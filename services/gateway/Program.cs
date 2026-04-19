using Nolivra.Gateway.Services;
using Nolivra.Gateway.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    service = "Nolivra Gateway",
    status = "running",
    utcTime = DateTime.UtcNow
}));

app.MapPost("/assistant/process", async (AssistantRequest request, AiService aiService) =>
{
    var result = await aiService.ProcessAsync(request.Input);

    AssistantIntentResult? parsed;

    try
    {
        parsed = JsonSerializer.Deserialize<AssistantIntentResult>(result,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    catch
    {
        parsed = null;
    }

    if (parsed is not null)
    {
        return Results.Ok(new
        {
            data = parsed,
            receivedAt = DateTime.UtcNow
        });
    }

    return Results.Ok(new
    {
        data = new
        {
            error = "invalid_json_from_ai",
            raw = result
        },
        receivedAt = DateTime.UtcNow
    });

});

app.Run();