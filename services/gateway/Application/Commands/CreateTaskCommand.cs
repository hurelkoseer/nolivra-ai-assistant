namespace Nolivra.Gateway.Application.Commands;

public sealed record CreateTaskCommand(
    string Title,
    string? Details,
    DateTimeOffset? DueAt);