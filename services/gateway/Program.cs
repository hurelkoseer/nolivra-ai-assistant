using Microsoft.EntityFrameworkCore;
using Nolivra.Gateway.Application.Commands;
using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Handlers.Task;
using Nolivra.Gateway.Infrastructure.Persistence;
using Nolivra.Gateway.Infrastructure.Repositories;
using Nolivra.Gateway.Models;
using Nolivra.Gateway.Routing;
using Nolivra.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<AiService>();

builder.Services.AddDbContext<AssistantDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<CreateTaskCommandHandler>();
builder.Services.AddScoped<IIntentHandler, TaskIntentHandler>();
builder.Services.AddScoped<IntentRouter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/assistant/process", async (
    AssistantRequest request,
    AiService aiService,
    IntentRouter intentRouter,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Input))
            return Results.BadRequest("Input is required.");

        var aiResult = await aiService.ProcessAsync(request.Input);
        var handledResult = await intentRouter.RouteAsync(aiResult, cancellationToken);

        return Results.Ok(new
        {
            data = aiResult,
            handled = handledResult,
            receivedAt = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);

        return Results.Problem(
            title: "Request failed",
            detail: ex.ToString(),
            statusCode: 500);
    }
});

app.Run();