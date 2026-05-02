using Microsoft.EntityFrameworkCore;
using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Domain.Entities;
using Nolivra.Gateway.Infrastructure.Persistence;

namespace Nolivra.Gateway.Infrastructure.Repositories;

public sealed class NoteRepository : INoteRepository
{
    private readonly AssistantDbContext _dbContext;

    public NoteRepository(AssistantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(NoteItem note, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notes.AddAsync(note, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<NoteItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<NoteItem>> GetAllAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        return _dbContext.Notes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(NoteItem note, CancellationToken cancellationToken = default)
    {
        _dbContext.Notes.Update(note);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.Events.Update(calendarEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(NoteItem note, CancellationToken cancellationToken = default)
    {
        _dbContext.Notes.Remove(note);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<NoteItem?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notes
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.Title.ToLower() == title.ToLower(), cancellationToken);
    }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
