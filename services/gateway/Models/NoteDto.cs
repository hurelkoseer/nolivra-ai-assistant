namespace Nolivra.Gateway.Models;

public sealed record NoteDto(
    Guid Id,
    string Title,
    string? Details,
    DateTimeOffset CreatedAtUtc);