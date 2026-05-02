using System.Text;
using System.Text.Json;
using Nolivra.Gateway.Models;

namespace Nolivra.Gateway.Services;

public sealed class AiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AiProcessingResult> ProcessAsync(string input)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is missing.");

        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var prompt = @$"
You are an AI assistant that extracts structured intent from user input.

Today is: {today}

Return ONLY valid JSON in this format:
{{
  ""intent"": ""task|event|note|update"",
  ""title"": ""string or null"",
  ""datetime"": ""ISO-8601 string or null"",
  ""details"": ""string or null"",
  ""entityType"": ""task|event|note or null"",
  ""targetTitle"": ""string or null"",
  ""targetId"": null,
  ""fieldsToUpdate"": {{
    ""title"": ""string or null"",
    ""datetime"": ""ISO-8601 string or null"",
    ""details"": ""string or null"",
    ""status"": ""Pending|Completed or null""
  }}
}}

Rules:
- Supported intents are only: task, event, note, update.
- Resolve relative dates like ""yarın"", ""bugün"", ""gelecek hafta"" using today's date.
- datetime must be ISO-8601 if present.
- title must be short and clear.
- details can be null.
- For task, note, and event: entityType, targetTitle, targetId, and fieldsToUpdate must be null.
- For update: title, datetime, and details must be null.
- For update: entityType must be one of task, event, note.
- For update: targetTitle must be the existing item title mentioned by the user.
- For update: targetId must be null unless user explicitly provides an id.
- For update: fieldsToUpdate must contain only the fields the user wants to change.
- For note updates, use details when the user wants to change note content.
- For task status updates, use status as Pending or Completed.
- Do not return markdown.
- Do not explain anything.

Examples:

User input: berbere git taskını completed yap
{{
  ""intent"": ""update"",
  ""title"": null,
  ""datetime"": null,
  ""details"": null,
  ""entityType"": ""task"",
  ""targetTitle"": ""Berbere git"",
  ""targetId"": null,
  ""fieldsToUpdate"": {{
    ""status"": ""Completed""
  }}
}}

User input: yarınki doktor randevumu 16:00 yap
{{
  ""intent"": ""update"",
  ""title"": null,
  ""datetime"": null,
  ""details"": null,
  ""entityType"": ""event"",
  ""targetTitle"": ""Doktor randevusu"",
  ""targetId"": null,
  ""fieldsToUpdate"": {{
    ""datetime"": ""{today}T16:00:00""
  }}
}}

User input: Market notunun detayını süt ve yumurta al olarak değiştir
{{
  ""intent"": ""update"",
  ""title"": null,
  ""datetime"": null,
  ""details"": null,
  ""entityType"": ""note"",
  ""targetTitle"": ""Market"",
  ""targetId"": null,
  ""fieldsToUpdate"": {{
    ""details"": ""süt ve yumurta al""
  }}
}}

User input: {input}
";

        var requestBody = new
        {
            model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You extract intent and return only JSON."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"OpenAI request failed: {response.StatusCode} - {responseContent}");

        using var jsonDoc = JsonDocument.Parse(responseContent);

        var content = jsonDoc
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        var result = ParseAssistantIntentResult(content);

        return new AiProcessingResult(content, result);
    }

    private static AssistantIntentResult ParseAssistantIntentResult(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return new AssistantIntentResult
            {
                Intent = "unknown",
                Title = "Empty AI response",
                Details = "AI returned an empty response."
            };
        }

        try
        {
            var cleanedJson = ExtractJson(rawResponse);

            var parsed = JsonSerializer.Deserialize<AssistantIntentResult>(
                cleanedJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return parsed ?? new AssistantIntentResult
            {
                Intent = "unknown",
                Title = "Invalid AI response",
                Details = rawResponse
            };
        }
        catch
        {
            return new AssistantIntentResult
            {
                Intent = "unknown",
                Title = "Invalid AI JSON",
                Details = rawResponse
            };
        }
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();

        if (trimmed.StartsWith("```json"))
        {
            trimmed = trimmed
                .Replace("```json", string.Empty)
                .Replace("```", string.Empty)
                .Trim();
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed
                .Replace("```", string.Empty)
                .Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');

        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return trimmed;
    }
}