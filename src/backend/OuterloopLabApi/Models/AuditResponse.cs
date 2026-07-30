namespace OuterloopLabApi.Models;

public sealed class AuditResponse
{
    public string AuditId { get; init; } = string.Empty;
    public string FromCurrency { get; init; } = string.Empty;
    public string ToCurrency { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal ExchangeRate { get; init; }
    public decimal ConvertedAmount { get; init; }
    public string ProviderDateMarker { get; init; } = string.Empty;
    public DateTime ExecutionTimestampUtc { get; init; }

    public AuditResponse(AuditRecord record)
    {
        AuditId = record.AuditId;
        FromCurrency = record.FromCurrency;
        ToCurrency = record.ToCurrency;
        Amount = record.Amount;
        ExchangeRate = record.ExchangeRate;
        ConvertedAmount = record.ConvertedAmount;
        ProviderDateMarker = record.ProviderDateMarker;
        ExecutionTimestampUtc = record.ExecutionTimestampUtc;
    }
}
