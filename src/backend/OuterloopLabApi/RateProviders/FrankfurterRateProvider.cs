using System.Text.Json;
using Microsoft.Extensions.Options;
using OuterloopLabApi.Models;

namespace OuterloopLabApi.RateProviders;

public sealed class FrankfurterRateProvider : ICurrencyRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly BackendOptions _options;

    public FrankfurterRateProvider(HttpClient httpClient, BackendOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ExchangeRate> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        var upstreamBase = NormalizeUpstreamBase(_options.CurrencyApiBaseUrl);
        var url = $"{upstreamBase}/v1/latest?base={Uri.EscapeDataString(fromCurrency)}&symbols={Uri.EscapeDataString(toCurrency)}";

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url, cancellationToken);
        }
        catch (Exception)
        {
            throw new RateProviderUnavailableException();
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new RateProviderUnavailableException();
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        JsonDocument doc;
        try
        {
            doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            throw new RateProviderUnavailableException();
        }

        var root = doc.RootElement;
        var mapped = ExchangeRateMapper.TryMap(root, toCurrency);
        if (mapped is null)
        {
            throw new RateProviderUnavailableException();
        }

        return mapped;
    }

    private static string NormalizeUpstreamBase(string configuredBase)
    {
        if (string.IsNullOrWhiteSpace(configuredBase))
        {
            return "https://api.frankfurter.dev";
        }

        // Known Constraints default points to https://frankfurter.dev, but Frankfurter's live JSON API is on api.frankfurter.dev.
        if (configuredBase.TrimEnd('/').Equals("https://frankfurter.dev", StringComparison.OrdinalIgnoreCase))
        {
            return "https://api.frankfurter.dev";
        }

        return configuredBase.TrimEnd('/');
    }
}
