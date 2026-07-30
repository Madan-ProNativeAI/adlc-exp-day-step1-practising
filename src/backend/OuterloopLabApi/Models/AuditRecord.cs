using System.Text.Json.Serialization;

namespace OuterloopLabApi.Models;

public sealed class AuditRecord
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("auditId")]
    public required string AuditId { get; init; }

    [JsonPropertyName("fromCurrency")]
    public required string FromCurrency { get; init; }

    [JsonPropertyName("toCurrency")]
    public required string ToCurrency { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("exchangeRate")]
    public required decimal ExchangeRate { get; init; }

    [JsonPropertyName("convertedAmount")]
    public required decimal ConvertedAmount { get; init; }

    [JsonPropertyName("providerDateMarker")]
    public required string ProviderDateMarker { get; init; }

    [JsonPropertyName("executionTimestampUtc")]
    public required DateTime ExecutionTimestampUtc { get; init; }
}
