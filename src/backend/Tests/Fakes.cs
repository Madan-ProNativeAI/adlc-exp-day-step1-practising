using OuterloopLabApi.Models;
using OuterloopLabApi.RateProviders;

namespace Tests;

internal sealed class FakeRateProviderSuccess : ICurrencyRateProvider
{
    private readonly ExchangeRate _rate;

    public FakeRateProviderSuccess(decimal rate, string providerDateMarker)
    {
        _rate = new ExchangeRate { Rate = rate, ProviderDateMarker = providerDateMarker };
    }

    public Task<ExchangeRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        return Task.FromResult(_rate);
    }
}

internal sealed class FakeRateProviderFailure : ICurrencyRateProvider
{
    public Task<ExchangeRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
        => throw new RateProviderUnavailableException();
}
