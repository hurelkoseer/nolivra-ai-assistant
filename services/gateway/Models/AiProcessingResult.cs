namespace Nolivra.Gateway.Models;

public sealed record AiProcessingResult(
    string RawResponse,
    AssistantIntentResult ParsedResult);
