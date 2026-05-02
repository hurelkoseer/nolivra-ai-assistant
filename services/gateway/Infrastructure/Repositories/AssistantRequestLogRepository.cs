namespace Nolivra.Gateway.Infrastructure.Repositories;

using Nolivra.Gateway.Domain.Entities;
using Nolivra.Gateway.Infrastructure.Persistence;

public interface IAssistantRequestLogRepository
{
    Task SaveAsync(AssistantRequestLog log, CancellationToken cancellationToken = default);
}

internal sealed class AssistantRequestLogRepository : IAssistantRequestLogRepository
{
    private readonly AssistantDbContext _context;

    public AssistantRequestLogRepository(AssistantDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(AssistantRequestLog log, CancellationToken cancellationToken = default)
    {
        _context.AssistantRequestLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
