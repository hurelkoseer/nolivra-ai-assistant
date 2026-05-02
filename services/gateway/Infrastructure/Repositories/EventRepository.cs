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

    public Task<List<CalendarEvent>> GetAllAsync(
     int page,
     int pageSize,
     DateTimeOffset? from = null,
     DateTimeOffset? to = null,
     CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Events
            .AsNoTracking()
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(x => x.StartAtUtc >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.StartAtUtc <= to.Value);
        }

        return query
            .OrderByDescending(x => x.StartAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.Events.Update(calendarEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.Events.Remove(calendarEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CalendarEvent?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        var normalized = title.Trim().ToLower();

        var exact = await _dbContext.Events
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(
                x => x.Title.ToLower() == normalized,
                cancellationToken);

        if (exact != null)
            return exact;

        return await _dbContext.Events
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(
                x => x.Title.ToLower().Contains(normalized),
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}