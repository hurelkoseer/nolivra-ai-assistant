using Nolivra.Gateway.Application.Commands;
using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Models;

namespace Nolivra.Gateway.Handlers.Task;

public sealed class TaskIntentHandler : IIntentHandler
{
    private readonly CreateTaskCommandHandler _createTaskCommandHandler;

    public TaskIntentHandler(CreateTaskCommandHandler createTaskCommandHandler)
    {
        _createTaskCommandHandler = createTaskCommandHandler;
    }

    public string SupportedIntent => "task";

    public async Task<HandleIntentResult> HandleAsync(AssistantIntentResult intent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(intent.Title))
            throw new InvalidOperationException("AI returned empty title.");

        DateTimeOffset? dueAt = null;

        if (!string.IsNullOrWhiteSpace(intent.Datetime))
        {
            if (DateTimeOffset.TryParse(intent.Datetime, out var parsedDate))
            {
                dueAt = parsedDate.ToUniversalTime();
            }
            else
            {
                throw new InvalidOperationException("Invalid datetime format returned by AI.");
            }
        }

        var command = new CreateTaskCommand(intent.Title, intent.Details, dueAt);
        var taskId = await _createTaskCommandHandler.HandleAsync(command, cancellationToken);

        return new HandleIntentResult
        {
            Success = true,
            EntityType = "task",
            EntityId = taskId,
            Message = "Task created successfully."
        };
    }
}