using OuterloopLabApi.Models;
using OuterloopLabApi.Repositories;
using OuterloopLabApi.Services;
using Xunit;

namespace Tests;

public sealed class CurrencyConversionServiceTests
{
    [Fact]
    public async Task PersistsAndReturnsAuditRecord()
    {
        var repo = new InMemoryAuditRepository();
        var rateProvider = new FakeRateProviderSuccess(0.9m, "2026-01-04");
        var service = new CurrencyConversionService(rateProvider, repo);

        var response = await service.ConvertAsync(new ConversionRequest
        {
            FromCurrency = "USD",
            ToCurrency = "EUR",
            Amount = 100m
        }, CancellationToken.None);

        Assert.NotNull(response.AuditId);
        Assert.Equal("USD", response.FromCurrency);
        Assert.Equal("EUR", response.ToCurrency);
        Assert.Equal(0.9m, response.ExchangeRate);
        Assert.Equal(90m, response.ConvertedAmount);

        var record = await repo.GetAsync(response.AuditId, CancellationToken.None);
        Assert.NotNull(record);
        Assert.Equal(90m, record!.ConvertedAmount);
        Assert.Equal("2026-01-04", record.ProviderDateMarker);
        Assert.Equal(response.ExecutionTimestampUtc, record.ExecutionTimestampUtc);
    }
}
