using Microsoft.EntityFrameworkCore;
using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Domain.Entities;
using Nolivra.Gateway.Infrastructure.Persistence;

namespace Nolivra.Gateway.Infrastructure.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AssistantDbContext _dbContext;

    public EventRepository(AssistantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        await _dbContext.Events.AddAsync(calendarEvent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
