namespace Nolivra.Gateway.Application.Services;

using Nolivra.Gateway.Domain.Entities;

public interface INoteRepository
{
    Task AddAsync(NoteItem note, CancellationToken cancellationToken = default);
    Task<NoteItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
