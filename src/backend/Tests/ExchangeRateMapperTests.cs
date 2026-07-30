using System.Text.Json;
using OuterloopLabApi.RateProviders;
using Xunit;

namespace Tests;

public sealed class ExchangeRateMapperTests
{
    [Fact]
    public void MapsRatesProperty()
    {
        var json = JsonDocument.Parse("{\"date\":\"2026-01-04\",\"rates\":{\"EUR\":0.87138}}").RootElement;

        var mapped = ExchangeRateMapper.TryMap(json, "EUR");

        Assert.NotNull(mapped);
        Assert.Equal(0.87138m, mapped!.Rate);
        Assert.Equal("2026-01-04", mapped.ProviderDateMarker);
    }

    [Fact]
    public void MapsConversionRatesProperty()
    {
        var json = JsonDocument.Parse("{\"date\":\"2026-01-04\",\"conversion_rates\":{\"EUR\":0.8}}").RootElement;

        var mapped = ExchangeRateMapper.TryMap(json, "EUR");

        Assert.NotNull(mapped);
        Assert.Equal(0.8m, mapped!.Rate);
        Assert.Equal("2026-01-04", mapped.ProviderDateMarker);
    }

    [Fact]
    public void MapsDirectRateProperty()
    {
        var json = JsonDocument.Parse("{\"date\":\"2026-01-04\",\"rate\":0.7}").RootElement;

        var mapped = ExchangeRateMapper.TryMap(json, "EUR");

        Assert.NotNull(mapped);
        Assert.Equal(0.7m, mapped!.Rate);
        Assert.Equal("2026-01-04", mapped.ProviderDateMarker);
    }
}
