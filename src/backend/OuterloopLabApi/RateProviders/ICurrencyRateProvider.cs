using OuterloopLabApi.RateProviders;

namespace OuterloopLabApi.RateProviders;

public interface ICurrencyRateProvider
{
    Task<ExchangeRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}
