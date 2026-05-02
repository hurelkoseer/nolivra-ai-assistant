namespace Nolivra.Gateway.Models;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Details,
    DateTimeOffset? DueAt,
    DateTimeOffset CreatedAt,
    string Status);