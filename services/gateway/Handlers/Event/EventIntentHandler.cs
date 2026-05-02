using Nolivra.Gateway.Application.Commands;
using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Models;

namespace Nolivra.Gateway.Handlers.Event;

public sealed class EventIntentHandler : IIntentHandler
{
    private readonly CreateEventCommandHandler _createEventCommandHandler;

    public EventIntentHandler(CreateEventCommandHandler createEventCommandHandler)
    {
        _createEventCommandHandler = createEventCommandHandler;
    }

    public string SupportedIntent => "event";

    public async Task<HandleIntentResult> HandleAsync(AssistantIntentResult intent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(intent.Title))
            throw new InvalidOperationException("AI returned empty title.");

        if (string.IsNullOrWhiteSpace(intent.Datetime))
            throw new InvalidOperationException("Event requires a start datetime.");

        DateTimeOffset startAt;
        if (!DateTimeOffset.TryParse(intent.Datetime, out startAt))
            throw new InvalidOperationException("Invalid start datetime format returned by AI.");

        DateTimeOffset? endAt = null;
        // For now, no end time from AI. Can extend later if needed.

        var command = new CreateEventCommand(intent.Title, intent.Details, startAt.ToUniversalTime(), endAt);
        var eventId = await _createEventCommandHandler.HandleAsync(command, cancellationToken);

        return new HandleIntentResult
        {
            Success = true,
            EntityType = "event",
            EntityId = eventId,
            Message = "Event created successfully."
        };
    }
}
