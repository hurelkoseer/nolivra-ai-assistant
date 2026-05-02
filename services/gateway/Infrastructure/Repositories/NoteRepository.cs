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

    public async Task<IReadOnlyList<NoteItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
