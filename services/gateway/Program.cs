using Microsoft.EntityFrameworkCore;
using Nolivra.Gateway.Application.Commands;
using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Application.Validators;
using Nolivra.Gateway.Domain.Entities;
using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Handlers.Event;
using Nolivra.Gateway.Handlers.Note;
using Nolivra.Gateway.Handlers.Task;
using Nolivra.Gateway.Infrastructure.Middleware;
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
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IAssistantRequestLogRepository, AssistantRequestLogRepository>();
builder.Services.AddScoped<AssistantIntentValidator>();
builder.Services.AddScoped<CreateTaskCommandHandler>();
builder.Services.AddScoped<CreateNoteCommandHandler>();
builder.Services.AddScoped<CreateEventCommandHandler>();
builder.Services.AddScoped<IIntentHandler, TaskIntentHandler>();
builder.Services.AddScoped<IIntentHandler, NoteIntentHandler>();
builder.Services.AddScoped<IIntentHandler, EventIntentHandler>();
builder.Services.AddScoped<IntentRouter>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/assistant/process", async (
    AssistantRequest request,
    AiService aiService,
    AssistantIntentValidator validator,
    IAssistantRequestLogRepository logRepository,
    IntentRouter intentRouter,
    CancellationToken cancellationToken) =>
{
    var log = new AssistantRequestLog
    {
        Id = Guid.NewGuid(),
        RawUserInput = request.Input,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    if (string.IsNullOrWhiteSpace(request.Input))
        return Results.BadRequest("Input is required.");

    var aiProcessing = await aiService.ProcessAsync(request.Input);
    log.RawAiResponse = aiProcessing.RawResponse;
    log.ParsedIntent = aiProcessing.ParsedResult.Intent;

    var validationResult = validator.Validate(aiProcessing.ParsedResult);

    if (!validationResult.IsValid)
    {
        log.Success = false;
        log.ErrorDetail = validationResult.ErrorMessage;
        await logRepository.SaveAsync(log, cancellationToken);

        return Results.BadRequest(new
        {
            error = validationResult.ErrorMessage
        });
    }

    var handledResult = await intentRouter.RouteAsync(aiProcessing.ParsedResult, cancellationToken);

    log.Success = true;
    await logRepository.SaveAsync(log, cancellationToken);

    return Results.Ok(new
    {
        data = aiProcessing.ParsedResult,
        handled = handledResult,
        receivedAt = DateTime.UtcNow
    });
});

app.MapGet("/tasks", async (
    ITaskRepository taskRepository,
    CancellationToken cancellationToken) =>
{
    var tasks = await taskRepository.GetAllAsync(cancellationToken);

    var dto = tasks.Select(x => new TaskDto(
        x.Id,
        x.Title,
        x.Details,
        x.DueAt,
        x.CreatedAt,
        x.Status));

    return Results.Ok(dto);
});

app.MapGet("/notes", async (
    INoteRepository noteRepository,
    CancellationToken cancellationToken) =>
{
    var notes = await noteRepository.GetAllAsync(cancellationToken);

    var dto = notes.Select(x => new NoteDto(
        x.Id,
        x.Title,
        x.Details,
        x.CreatedAtUtc));

    return Results.Ok(dto);
});

app.MapGet("/events", async (
    IEventRepository eventRepository,
    CancellationToken cancellationToken) =>
{
    var events = await eventRepository.GetAllAsync(cancellationToken);

    var response = events.Select(x => new EventDto(
        x.Id,
        x.Title,
        x.Details,
        x.StartAtUtc,
        x.EndAtUtc,
        x.CreatedAtUtc));

    return Results.Ok(response);
});

app.Run();
