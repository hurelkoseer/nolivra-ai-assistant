namespace Nolivra.Gateway.Models;

public record AssistantRequest(
    string Input,
    string? Locale,
    string? Timezone
);