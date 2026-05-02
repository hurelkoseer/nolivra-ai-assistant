using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Services;

public interface INoteRepository
{
    Task AddAsync(NoteItem note, CancellationToken cancellationToken = default);
    Task<NoteItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NoteItem>> GetAllAsync(CancellationToken cancellationToken = default);
}