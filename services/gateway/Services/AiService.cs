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
  ""intent"": ""task|event|note|wake_alert|email_action"",
  ""title"": ""string"",
  ""datetime"": ""ISO-8601 string or null"",
  ""details"": ""string or null""
}}

Rules:
- Resolve relative dates like ""yarın"", ""bugün"", ""gelecek hafta"" using today's date.
- datetime must be ISO-8601 if present.
- title must be short and clear.
- details can be null.
- Do not return markdown.
- Do not explain anything.

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