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
        if (string.IsNullOrWhiteSpace(result.Intent))
            return ValidationResult.Failure("Intent is required");

        if (!ValidIntents.Contains(result.Intent))
            return ValidationResult.Failure($"Intent '{result.Intent}' is not supported");

        if (string.IsNullOrWhiteSpace(result.Title))
            return ValidationResult.Failure("Title is required");

        if (!string.IsNullOrWhiteSpace(result.Datetime))
        {
            if (!DateTimeOffset.TryParse(result.Datetime, out _))
                return ValidationResult.Failure($"DateTime format is invalid: {result.Datetime}");
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
