using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Microsoft.Azure.Cosmos;
 

namespace OuterloopLabApi.Services;

public sealed class CosmosProvisioningService
{
    private readonly BackendOptions _options;
    private readonly DefaultAzureCredential _credential;
    private readonly CosmosClient _cosmosClient;

    public CosmosProvisioningService(BackendOptions options, DefaultAzureCredential credential, CosmosClient cosmosClient)
    {
        _options = options;
        _credential = credential;
        _cosmosClient = cosmosClient;
    }

    public async Task ProvisionOrThrowAsync(CancellationToken stoppingToken)
    {
        await TryArmProvisionAsync(stoppingToken);

        // Constraint: token-authenticated data-plane create-if-not-exists must run and must fail startup on error.
        var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
            _options.CosmosDbDatabase,
            cancellationToken: stoppingToken);

        var containerProps = new ContainerProperties(_options.CosmosDbContainer, "/auditId");
        await databaseResponse.Database.CreateContainerIfNotExistsAsync(
            containerProps,
            throughput: 400,
            cancellationToken: stoppingToken);
    }

    private async Task TryArmProvisionAsync(CancellationToken stoppingToken)
    {
        // ARM provisioning is best-effort; Managed Identity RBAC for ARM may differ from data-plane RBAC.
        try
        {
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                return;
            }

            var armClient = new ArmClient(_credential, subscriptionId);

            // ARM SDK methods require full resource identifiers.
            var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{_options.CosmosDbResourceGroup}");
            var resourceGroup = armClient.GetResourceGroupResource(resourceGroupId);

            var accountResponse = await resourceGroup.GetCosmosDBAccounts().GetAsync(_options.CosmosDbAccountName, stoppingToken);
            var account = accountResponse.Value;

            var location = new AzureLocation(_options.CosmosDbRegion);

            // Create the SQL database (control plane best-effort).
            var dbCollection = account.GetCosmosDBSqlDatabases();
            var dbContent = new CosmosDBSqlDatabaseCreateOrUpdateContent(
                location,
                new CosmosDBSqlDatabaseResourceInfo(_options.CosmosDbDatabase));

            await dbCollection.CreateOrUpdateAsync(
                WaitUntil.Completed,
                _options.CosmosDbDatabase,
                dbContent,
                stoppingToken);

            // Create the SQL container (control plane best-effort).
            var dbResponse = await account.GetCosmosDBSqlDatabaseAsync(_options.CosmosDbDatabase, stoppingToken);
            var dbResource = dbResponse.Value;
            var containerCollection = dbResource.GetCosmosDBSqlContainers();

            // ARM provisioning is best-effort; we keep this configuration minimal to avoid SDK shape drift.
            var partitionKey = new CosmosDBContainerPartitionKey();

            var indexingPolicy = new CosmosDBIndexingPolicy();
            var containerInfo = new CosmosDBSqlContainerResourceInfo(_options.CosmosDbContainer)
            {
                IndexingPolicy = indexingPolicy,
                PartitionKey = partitionKey,
                DefaultTtl = null,
                ConflictResolutionPolicy = null!,
                AnalyticalStorageTtl = null
            };

            var containerContent = new CosmosDBSqlContainerCreateOrUpdateContent(location, containerInfo);

            await containerCollection.CreateOrUpdateAsync(
                WaitUntil.Completed,
                _options.CosmosDbContainer,
                containerContent,
                stoppingToken);
        }
        catch
        {
            // Best effort only.
        }
    }
}
