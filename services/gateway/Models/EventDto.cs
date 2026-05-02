namespace Nolivra.Gateway.Models;

public sealed record EventDto(
    Guid Id,
    string Title,
    string? Details,
    DateTimeOffset StartAtUtc,
    DateTimeOffset? EndAtUtc,
    DateTimeOffset CreatedAtUtc);