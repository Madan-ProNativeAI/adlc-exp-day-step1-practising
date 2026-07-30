namespace OuterloopLabApi.RateProviders;

public sealed class ExchangeRate
{
    public required decimal Rate { get; init; }
    public required string ProviderDateMarker { get; init; }
}
