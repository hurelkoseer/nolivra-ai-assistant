using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Services;

public interface INoteRepository
{
    Task AddAsync(NoteItem note, CancellationToken cancellationToken = default);
    Task<NoteItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NoteItem>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(NoteItem note, CancellationToken cancellationToken = default);
    Task DeleteAsync(NoteItem note, CancellationToken cancellationToken = default);
    Task<NoteItem?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}