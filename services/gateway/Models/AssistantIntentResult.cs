namespace Nolivra.Gateway.Models;

public sealed class AssistantIntentResult
{
    public string Intent { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Datetime { get; set; }
    public string? Details { get; set; }
}