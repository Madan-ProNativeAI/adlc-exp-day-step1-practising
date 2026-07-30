using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Models;
using OuterloopLabApi.RateProviders;
using OuterloopLabApi.Services;

namespace OuterloopLabApi.Controllers;

[ApiController]
[Route("api/conversions")]
public sealed class ConversionsController : ControllerBase
{
    private readonly CurrencyConversionService _conversionService;

    public ConversionsController(CurrencyConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    [HttpPost]
    public async Task<IActionResult> ConvertAsync([FromBody] ConversionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Problem(statusCode: 400, title: "Invalid request", detail: "Request body validation failed.");
        }

        try
        {
            var result = await _conversionService.ConvertAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (RateProviderUnavailableException)
        {
            var problem = new ProblemDetails
            {
                Status = 503,
                Title = "Rate provider unavailable",
                Type = "https://example.com/errors/rate-provider-unavailable"
            };
            return StatusCode(503, problem);
        }
        catch (Exception ex)
        {
            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "Conversion failed",
                Detail = "An unexpected error occurred."
            };
            // Avoid leaking raw exception details.
            _ = ex;
            return StatusCode(500, problem);
        }
    }
}
