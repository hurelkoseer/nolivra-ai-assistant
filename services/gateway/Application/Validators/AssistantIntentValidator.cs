namespace Nolivra.Gateway.Application.Validators;

using Nolivra.Gateway.Models;

public sealed class AssistantIntentValidator
{
    private static readonly HashSet<string> ValidIntents = new()
    {
        "task",
        "event",
        "note",
        "wake_alert",
        "email_action"
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

        if (result.Intent is "task" or "event" or "wake_alert")
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
