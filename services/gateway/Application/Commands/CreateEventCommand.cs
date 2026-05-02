namespace Nolivra.Gateway.Application.Commands;

public sealed record CreateEventCommand(
    string Title,
    string? Details,
    DateTimeOffset StartAtUtc,
    DateTimeOffset? EndAtUtc);
