using OuterloopLabApi.Models;

namespace OuterloopLabApi.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditRecord record, CancellationToken cancellationToken);
    Task<AuditRecord?> GetAsync(string auditId, CancellationToken cancellationToken);
}
