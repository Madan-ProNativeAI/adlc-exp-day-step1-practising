using System.Text.Json;

namespace OuterloopLabApi.RateProviders;

public static class ExchangeRateMapper
{
    public static ExchangeRate? TryMap(JsonElement root, string toCurrency)
    {
        string? providerDate = TryGetString(root, "date")
                                 ?? TryGetString(root, "providerDate")
                                 ?? TryGetString(root, "effectiveDate");

        if (providerDate is null)
        {
            providerDate = string.Empty;
        }

        // Flexible mapping: support multiple possible property shapes.
        decimal? rate = TryGetRateFromObject(root, "rates", toCurrency)
                      ?? TryGetRateFromObject(root, "conversion_rates", toCurrency)
                      ?? TryGetRateFromObject(root, "conversionRates", toCurrency);

        if (rate is null && root.TryGetProperty("rate", out var directRate) && directRate.ValueKind == JsonValueKind.Number)
        {
            rate = directRate.GetDecimal();
        }

        if (rate is null)
        {
            return null;
        }

        return new ExchangeRate
        {
            Rate = rate.Value,
            ProviderDateMarker = providerDate
        };
    }

    private static decimal? TryGetRateFromObject(JsonElement root, string containerPropertyName, string toCurrency)
    {
        if (!root.TryGetProperty(containerPropertyName, out var container) || container.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var symbol = toCurrency.Trim().ToUpperInvariant();
        if (container.TryGetProperty(symbol, out var value) && value.ValueKind == JsonValueKind.Number)
        {
            return value.GetDecimal();
        }

        // Case-insensitive fallback: iterate keys.
        foreach (var prop in container.EnumerateObject())
        {
            if (prop.NameEquals(symbol))
            {
                if (prop.Value.ValueKind == JsonValueKind.Number)
                {
                    return prop.Value.GetDecimal();
                }
            }
        }

        return null;
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
        }
        return null;
    }
}
