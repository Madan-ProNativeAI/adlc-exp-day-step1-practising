using System.Text.Json;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.RateProviders;
using OuterloopLabApi.Repositories;
using OuterloopLabApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Constraint: read configuration exclusively from runtime environment variables.
builder.Configuration.Sources.Clear();

builder.Services.AddProblemDetails();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var backendOptions = BackendOptions.FromEnvironment();
builder.Services.AddSingleton(backendOptions);

builder.Services.AddSingleton<DefaultAzureCredential>(sp =>
{
    var opts = new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = backendOptions.AzureManagedIdentityClientId
    };
    return new DefaultAzureCredential(opts);
});

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var credential = sp.GetRequiredService<DefaultAzureCredential>();
    var cosmosOptions = new CosmosClientOptions
    {
        ApplicationName = "currency-conversion-audit-trail"
    };
    return new CosmosClient(backendOptions.CosmosDbUri, credential, cosmosOptions);
});

builder.Services.AddHttpClient<FrankfurterRateProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<ICurrencyRateProvider, FrankfurterRateProvider>();
builder.Services.AddScoped<IAuditRepository, CosmosAuditRepository>();
builder.Services.AddScoped<CurrencyConversionService>();

builder.Services.AddSingleton<CosmosProvisioningService>();

var app = builder.Build();

// Provision before the web app runs. If data-plane create-if-not-exists fails, startup must fail.
using (var scope = app.Services.CreateScope())
{
    var provisioning = scope.ServiceProvider.GetRequiredService<CosmosProvisioningService>();
    await provisioning.ProvisionOrThrowAsync(app.Lifetime.ApplicationStopping);
}

app.UseRouting();
app.MapControllers();

app.Run();

public sealed class BackendOptions
{
    public required string CosmosDbUri { get; init; }
    public required string CosmosDbDatabase { get; init; }
    public required string CosmosDbContainer { get; init; }
    public required string CosmosDbAccountName { get; init; }
    public required string CosmosDbResourceGroup { get; init; }
    public required string CosmosDbRegion { get; init; }
    public required string AzureManagedIdentityClientId { get; init; }
    public required string CurrencyApiBaseUrl { get; init; }

    public static BackendOptions FromEnvironment()
    {
        // Constraint: use the exact keys from docs\CONTAINER_ENVIRONMENT_VARIABLES.md
        static string ReadRequired(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing required environment variable: {key}");
            }
            return value;
        }

        // Not listed in docs, but required by Known Constraints.
        var currencyApiBaseUrl = Environment.GetEnvironmentVariable("CURRENCY_API_BASE_URL")
                                  ?? "https://frankfurter.dev";

        return new BackendOptions
        {
            CosmosDbUri = ReadRequired("COSMOS_DB_URI"),
            CosmosDbDatabase = ReadRequired("COSMOS_DB_DATABASE"),
            CosmosDbContainer = ReadRequired("COSMOS_DB_CONTAINER"),
            CosmosDbAccountName = ReadRequired("COSMOS_DB_ACCOUNT_NAME"),
            CosmosDbResourceGroup = ReadRequired("COSMOS_DB_RESOURCE_GROUP"),
            CosmosDbRegion = ReadRequired("COSMOS_DB_REGION"),
            AzureManagedIdentityClientId = ReadRequired("AZURE_MANAGED_IDENTITY_CLIENT_ID"),
            CurrencyApiBaseUrl = currencyApiBaseUrl
        };
    }
}

namespace OuterloopLabApi
{
    // marker namespace for compilation units that live next to Program.cs
}
