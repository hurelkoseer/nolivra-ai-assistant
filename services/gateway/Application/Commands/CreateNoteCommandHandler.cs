using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Application.Commands;

public sealed class CreateNoteCommandHandler
{
    private readonly INoteRepository _noteRepository;

    public CreateNoteCommandHandler(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<Guid> HandleAsync(CreateNoteCommand command, CancellationToken cancellationToken = default)
    {
        var note = NoteItem.Create(command.Title, command.Details);
        await _noteRepository.AddAsync(note, cancellationToken);
        return note.Id;
    }
}
