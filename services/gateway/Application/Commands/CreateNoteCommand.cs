namespace Nolivra.Gateway.Application.Commands;

public sealed record CreateNoteCommand(
    string Title,
    string? Details);
