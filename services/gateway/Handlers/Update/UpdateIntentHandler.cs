using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Models;

namespace Nolivra.Gateway.Handlers.Update;

public sealed class UpdateIntentHandler : IIntentHandler
{
    private readonly ITaskRepository _taskRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IEventRepository _eventRepository;

    public UpdateIntentHandler(
        ITaskRepository taskRepository,
        INoteRepository noteRepository,
        IEventRepository eventRepository)
    {
        _taskRepository = taskRepository;
        _noteRepository = noteRepository;
        _eventRepository = eventRepository;
    }

    public string SupportedIntent => "update";

    public async System.Threading.Tasks.Task<HandleIntentResult> HandleAsync(
        AssistantIntentResult intent,
        CancellationToken cancellationToken = default)
    {
        if (intent.EntityType == "task")
        {
            return await HandleTaskUpdateAsync(intent, cancellationToken);
        }
        if (intent.EntityType == "note")
        {
            return await HandleNoteUpdateAsync(intent, cancellationToken);
        }
        if (intent.EntityType == "event")
        {
            return await HandleEventUpdateAsync(intent, cancellationToken);
        }

        return new HandleIntentResult
        {
            Success = false,
            EntityType = intent.EntityType ?? "unknown",
            Message = $"Update for entityType '{intent.EntityType}' is not implemented yet."
        };
    }

    private async System.Threading.Tasks.Task<HandleIntentResult> HandleTaskUpdateAsync(
        AssistantIntentResult intent,
        CancellationToken cancellationToken)
    {
        var task = intent.TargetId.HasValue
            ? await _taskRepository.GetByIdAsync(intent.TargetId.Value, cancellationToken)
            : await _taskRepository.GetByTitleAsync(intent.TargetTitle!, cancellationToken);

        if (task is null)
        {
            return new HandleIntentResult
            {
                Success = false,
                EntityType = "task",
                Message = $"Task not found: {intent.TargetTitle ?? intent.TargetId?.ToString()}"
            };
        }

        intent.FieldsToUpdate ??= new Dictionary<string, string?>();

        intent.FieldsToUpdate.TryGetValue("title", out var title);
        intent.FieldsToUpdate.TryGetValue("details", out var details);
        intent.FieldsToUpdate.TryGetValue("status", out var status);
        intent.FieldsToUpdate.TryGetValue("datetime", out var datetime);

        DateTimeOffset? dueAt = null;

        if (!string.IsNullOrWhiteSpace(datetime))
        {
            if (!DateTimeOffset.TryParse(datetime, out var parsedDate))
            {
                return new HandleIntentResult
                {
                    Success = false,
                    EntityType = "task",
                    EntityId = task.Id,
                    Message = $"Invalid datetime format: {datetime}"
                };
            }

            dueAt = parsedDate.ToUniversalTime();
        }

        task.Update(title, details, dueAt, status);

        await _taskRepository.SaveChangesAsync(cancellationToken);

        return new HandleIntentResult
        {
            Success = true,
            EntityType = "task",
            EntityId = task.Id,
            Message = "Task updated successfully."
        };
    }

    private async System.Threading.Tasks.Task<HandleIntentResult> HandleNoteUpdateAsync(
    AssistantIntentResult intent,
    CancellationToken cancellationToken)
    {
        var note = intent.TargetId.HasValue
            ? await _noteRepository.GetByIdAsync(intent.TargetId.Value, cancellationToken)
            : await _noteRepository.GetByTitleAsync(intent.TargetTitle!, cancellationToken);

        if (note is null)
        {
            return new HandleIntentResult
            {
                Success = false,
                EntityType = "note",
                Message = $"Note not found: {intent.TargetTitle ?? intent.TargetId?.ToString()}"
            };
        }

        intent.FieldsToUpdate ??= new Dictionary<string, string?>();

        intent.FieldsToUpdate.TryGetValue("title", out var title);
        intent.FieldsToUpdate.TryGetValue("details", out var details);

        note.Update(title, details);

        await _noteRepository.SaveChangesAsync(cancellationToken);

        return new HandleIntentResult
        {
            Success = true,
            EntityType = "note",
            EntityId = note.Id,
            Message = "Note updated successfully."
        };
    }

    private async System.Threading.Tasks.Task<HandleIntentResult> HandleEventUpdateAsync(
    AssistantIntentResult intent,
    CancellationToken cancellationToken)
    {
        var calendarEvent = intent.TargetId.HasValue
            ? await _eventRepository.GetByIdAsync(intent.TargetId.Value, cancellationToken)
            : await _eventRepository.GetByTitleAsync(intent.TargetTitle!, cancellationToken);

        if (calendarEvent is null)
        {
            return new HandleIntentResult
            {
                Success = false,
                EntityType = "event",
                Message = $"Event not found: {intent.TargetTitle ?? intent.TargetId?.ToString()}"
            };
        }

        intent.FieldsToUpdate ??= new Dictionary<string, string?>();

        intent.FieldsToUpdate.TryGetValue("title", out var title);
        intent.FieldsToUpdate.TryGetValue("details", out var details);
        intent.FieldsToUpdate.TryGetValue("datetime", out var datetime);

        DateTimeOffset? startAt = null;
        DateTimeOffset? endAt = null;

        if (!string.IsNullOrWhiteSpace(datetime))
        {
            if (!DateTimeOffset.TryParse(datetime, out var parsedDate))
            {
                return new HandleIntentResult
                {
                    Success = false,
                    EntityType = "event",
                    EntityId = calendarEvent.Id,
                    Message = $"Invalid datetime format: {datetime}"
                };
            }

            startAt = parsedDate.ToUniversalTime();
        }

        calendarEvent.Update(title, details, startAt, endAt);

        await _eventRepository.SaveChangesAsync(cancellationToken);

        return new HandleIntentResult
        {
            Success = true,
            EntityType = "event",
            EntityId = calendarEvent.Id,
            Message = "Event updated successfully."
        };
    }
}