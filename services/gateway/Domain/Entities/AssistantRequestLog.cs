namespace Nolivra.Gateway.Domain.Entities;

public class AssistantRequestLog
{
    public Guid Id { get; set; }
    public string RawUserInput { get; set; } = string.Empty;
    public string RawAiResponse { get; set; } = string.Empty;
    public string? ParsedIntent { get; set; }
    public bool Success { get; set; }
    public string? ErrorDetail { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
