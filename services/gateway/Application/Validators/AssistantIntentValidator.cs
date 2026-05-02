namespace Nolivra.Gateway.Application.Validators;

using Nolivra.Gateway.Models;

public sealed class AssistantIntentValidator
{
    private static readonly HashSet<string> ValidIntents = new()
    {
        "task",
        "event",
        "note",
        "update"
    };

    private static readonly HashSet<string> ValidEntityTypes = new()
    {
        "task",
        "event",
        "note"
    };

    public ValidationResult Validate(AssistantIntentResult result)
    {
        if (result is null)
        {
            return ValidationResult.Failure("AI result is required.");
        }

        if (string.IsNullOrWhiteSpace(result.Intent))
        {
            return ValidationResult.Failure("Intent is required.");
        }

        result.Intent = result.Intent.Trim().ToLowerInvariant();

        if (!ValidIntents.Contains(result.Intent))
        {
            return ValidationResult.Failure($"Intent '{result.Intent}' is not supported.");
        }

        if (result.Intent == "update")
        {
            return ValidateUpdateIntent(result);
        }

        if (string.IsNullOrWhiteSpace(result.Title))
        {
            return ValidationResult.Failure("Title is required.");
        }

        result.Title = result.Title.Trim();

        if (!string.IsNullOrWhiteSpace(result.Datetime))
        {
            if (!DateTimeOffset.TryParse(result.Datetime, out _))
            {
                return ValidationResult.Failure($"DateTime format is invalid: {result.Datetime}");
            }
        }

        if (result.Intent is "task" or "event")
        {
            if (string.IsNullOrWhiteSpace(result.Datetime))
            {
                return ValidationResult.Failure($"Datetime is required for intent '{result.Intent}'.");
            }
        }

        if (result.Intent == "note")
        {
            result.Datetime = null;
        }

        return ValidationResult.Success();
    }

    private static ValidationResult ValidateUpdateIntent(AssistantIntentResult result)
    {
        result.Title = null;
        result.Datetime = null;
        result.Details = null;

        if (string.IsNullOrWhiteSpace(result.EntityType))
        {
            return ValidationResult.Failure("EntityType is required for update intent.");
        }

        result.EntityType = result.EntityType.Trim().ToLowerInvariant();

        if (!ValidEntityTypes.Contains(result.EntityType))
        {
            return ValidationResult.Failure($"EntityType '{result.EntityType}' is not supported.");
        }

        if (string.IsNullOrWhiteSpace(result.TargetTitle) && result.TargetId is null)
        {
            return ValidationResult.Failure("TargetTitle or TargetId is required for update intent.");
        }

        if (!string.IsNullOrWhiteSpace(result.TargetTitle))
        {
            result.TargetTitle = result.TargetTitle.Trim();
        }

        if (result.FieldsToUpdate is null || result.FieldsToUpdate.Count == 0)
        {
            return ValidationResult.Failure("FieldsToUpdate is required for update intent.");
        }

        var cleanedFields = result.FieldsToUpdate
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(
                x => x.Key.Trim().ToLowerInvariant(),
                x => x.Value?.Trim()
            );

        if (cleanedFields.Count == 0)
        {
            return ValidationResult.Failure("FieldsToUpdate must contain at least one valid field.");
        }

        if (cleanedFields.TryGetValue("datetime", out var datetime) &&
            !DateTimeOffset.TryParse(datetime, out _))
        {
            return ValidationResult.Failure($"DateTime format is invalid: {datetime}");
        }

        if (cleanedFields.TryGetValue("status", out var status))
        {
            if (status is not "Pending" and not "Completed")
            {
                return ValidationResult.Failure($"Status '{status}' is not supported.");
            }
        }

        result.FieldsToUpdate = cleanedFields;

        return ValidationResult.Success();
    }
}

public sealed class ValidationResult
{
    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    private ValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true, null);

    public static ValidationResult Failure(string message) => new(false, message);
}