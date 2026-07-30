namespace OuterloopLabApi.Models;

public sealed class ConversionResponse
{
    public required string AuditId { get; init; }
    public required string FromCurrency { get; init; }
    public required string ToCurrency { get; init; }
    public required decimal Amount { get; init; }
    public required decimal ExchangeRate { get; init; }
    public required decimal ConvertedAmount { get; init; }
    public required string ProviderDateMarker { get; init; }
    public required DateTime ExecutionTimestampUtc { get; init; }
}
