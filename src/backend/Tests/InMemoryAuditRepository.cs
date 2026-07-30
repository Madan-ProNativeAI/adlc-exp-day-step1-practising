using OuterloopLabApi.Models;
using OuterloopLabApi.Repositories;

namespace Tests;

internal sealed class InMemoryAuditRepository : IAuditRepository
{
    private readonly Dictionary<string, AuditRecord> _items = new();

    public Task AddAsync(AuditRecord record, CancellationToken cancellationToken)
    {
        _items[record.AuditId] = record;
        return Task.CompletedTask;
    }

    public Task<AuditRecord?> GetAsync(string auditId, CancellationToken cancellationToken)
    {
        _items.TryGetValue(auditId, out var record);
        return Task.FromResult(record);
    }
}
