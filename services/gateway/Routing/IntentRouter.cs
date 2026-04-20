using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Models;

namespace Nolivra.Gateway.Routing;

public sealed class IntentRouter
{
    private readonly IReadOnlyDictionary<string, IIntentHandler> _handlers;

    public IntentRouter(IEnumerable<IIntentHandler> handlers)
    {
        _handlers = handlers.ToDictionary(x => x.SupportedIntent, StringComparer.OrdinalIgnoreCase);
    }

    public Task<HandleIntentResult> RouteAsync(AssistantIntentResult intent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(intent.Intent))
            throw new InvalidOperationException("Intent is empty.");

        if (!_handlers.TryGetValue(intent.Intent, out var handler))
            throw new InvalidOperationException($"No handler found for intent: {intent.Intent}");

        return handler.HandleAsync(intent, cancellationToken);
    }
}