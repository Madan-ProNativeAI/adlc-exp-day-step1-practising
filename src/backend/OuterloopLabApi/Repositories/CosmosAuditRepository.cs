using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Models;

namespace OuterloopLabApi.Repositories;

public sealed class CosmosAuditRepository : IAuditRepository
{
    private readonly Container _container;

    public CosmosAuditRepository(CosmosClient cosmosClient, BackendOptions options)
    {
        _container = cosmosClient.GetContainer(options.CosmosDbDatabase, options.CosmosDbContainer);
    }

    public async Task AddAsync(AuditRecord record, CancellationToken cancellationToken)
    {
        await _container.CreateItemAsync(record, new PartitionKey(record.AuditId), cancellationToken: cancellationToken);
    }

    public async Task<AuditRecord?> GetAsync(string auditId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<AuditRecord>(auditId, new PartitionKey(auditId), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
