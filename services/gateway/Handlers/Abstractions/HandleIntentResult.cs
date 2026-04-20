namespace Nolivra.Gateway.Handlers.Abstractions;

public sealed class HandleIntentResult
{
    public bool Success { get; init; }
    public string EntityType { get; init; } = default!;
    public Guid EntityId { get; init; }
    public string Message { get; init; } = default!;
}