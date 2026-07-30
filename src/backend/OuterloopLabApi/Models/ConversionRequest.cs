using System.ComponentModel.DataAnnotations;

namespace OuterloopLabApi.Models;

public sealed class ConversionRequest
{
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string FromCurrency { get; init; } = "";

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string ToCurrency { get; init; } = "";

    [Required]
    [Range(typeof(decimal), "0.0000000001", "79228162514264337593543950335")]
    public decimal Amount { get; init; }
}
