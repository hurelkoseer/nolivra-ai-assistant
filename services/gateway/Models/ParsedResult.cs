namespace Nolivra.Gateway.Models;

public sealed class ParsedResult
{
    public string? Intent { get; set; }
    public string? Title { get; set; }
    public string? Datetime { get; set; }
    public string? Details { get; set; }

    public string? EntityType { get; set; }
    public string? TargetTitle { get; set; }
    public Guid? TargetId { get; set; }
    public Dictionary<string, string?>? FieldsToUpdate { get; set; }
}