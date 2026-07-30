using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Models;
using OuterloopLabApi.Repositories;

namespace OuterloopLabApi.Controllers;

[ApiController]
[Route("api/audits")]
public sealed class AuditsController : ControllerBase
{
    private readonly IAuditRepository _auditRepository;

    public AuditsController(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    [HttpGet("{auditId}")]
    public async Task<IActionResult> GetAuditAsync([FromRoute] string auditId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(auditId))
        {
            var problem = new ProblemDetails
            {
                Status = 400,
                Title = "Invalid auditId"
            };
            return BadRequest(problem);
        }

        var record = await _auditRepository.GetAsync(auditId, cancellationToken);
        if (record is null)
        {
            var problem = new ProblemDetails
            {
                Status = 404,
                Title = "Audit record not found"
            };
            return NotFound(problem);
        }

        return Ok(new AuditResponse(record));
    }
}
