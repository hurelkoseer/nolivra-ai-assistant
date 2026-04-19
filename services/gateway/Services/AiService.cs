using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Nolivra.Gateway.Services;

public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AiService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["OpenAI:ApiKey"]!;
    }

    public async Task<string> ProcessAsync(string input)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
You are an assistant that extracts user intent.

Return only valid JSON.
Do not explain anything.
Do not use markdown.

Schema:
{
  "intent": "note|task|event|wake_alert|email_action",
  "title": "string",
  "datetime": "string|null",
  "details": "string|null"
}
"""
                },
                new
                {
                    role = "user",
                    content = input
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return JsonSerializer.Serialize(new
            {
                error = "openai_request_failed",
                statusCode = (int)response.StatusCode,
                response = responseContent
            });
        }

        using var doc = JsonDocument.Parse(responseContent);

        if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
        {
            return JsonSerializer.Serialize(new
            {
                error = "choices_not_found",
                response = responseContent
            });
        }

        var firstChoice = choices[0];

        if (!firstChoice.TryGetProperty("message", out var message))
        {
            return JsonSerializer.Serialize(new
            {
                error = "message_not_found",
                response = responseContent
            });
        }

        if (!message.TryGetProperty("content", out var contentElement))
        {
            return JsonSerializer.Serialize(new
            {
                error = "content_not_found",
                response = responseContent
            });
        }

        var content = contentElement.GetString();

        return string.IsNullOrWhiteSpace(content)
            ? JsonSerializer.Serialize(new
            {
                error = "content_empty",
                response = responseContent
            })
            : content;
    }
}