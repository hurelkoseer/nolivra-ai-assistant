namespace Nolivra.Gateway.Handlers.Note;

using Nolivra.Gateway.Application.Commands;
using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Models;

public sealed class NoteIntentHandler : IIntentHandler
{
    private readonly CreateNoteCommandHandler _createNoteCommandHandler;

    public NoteIntentHandler(CreateNoteCommandHandler createNoteCommandHandler)
    {
        _createNoteCommandHandler = createNoteCommandHandler;
    }

    public string SupportedIntent => "note";

    public async Task<HandleIntentResult> HandleAsync(AssistantIntentResult intent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(intent.Title))
            throw new InvalidOperationException("AI returned empty title.");

        var command = new CreateNoteCommand(intent.Title, intent.Details);
        var noteId = await _createNoteCommandHandler.HandleAsync(command, cancellationToken);

        return new HandleIntentResult
        {
            Success = true,
            EntityType = "note",
            EntityId = noteId,
            Message = "Note created successfully."
        };
    }
}
