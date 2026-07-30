using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Controllers;
using OuterloopLabApi.Models;
using OuterloopLabApi.Repositories;
using OuterloopLabApi.Services;
using Xunit;

namespace Tests;

public sealed class ConversionsControllerTests
{
    [Fact]
    public async Task Returns503WhenRateProviderFails()
    {
        var repo = new InMemoryAuditRepository();
        var rateProvider = new FakeRateProviderFailure();
        var service = new CurrencyConversionService(rateProvider, repo);
        var controller = new ConversionsController(service);

        var result = await controller.ConvertAsync(new ConversionRequest
        {
            FromCurrency = "USD",
            ToCurrency = "EUR",
            Amount = 100m
        }, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
        Assert.IsType<ProblemDetails>(objectResult.Value);
        var problem = (ProblemDetails)objectResult.Value!;
        Assert.Equal(503, problem.Status);
        Assert.Equal("https://example.com/errors/rate-provider-unavailable", problem.Type);
    }
}
