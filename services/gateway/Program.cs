using Microsoft.EntityFrameworkCore;
using Nolivra.Gateway.Application.Commands;
using Nolivra.Gateway.Application.Services;
using Nolivra.Gateway.Application.Validators;
using Nolivra.Gateway.Domain.Entities;
using Nolivra.Gateway.Handlers.Abstractions;
using Nolivra.Gateway.Handlers.Event;
using Nolivra.Gateway.Handlers.Note;
using Nolivra.Gateway.Handlers.Task;
using Nolivra.Gateway.Handlers.Update;
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
builder.Services.AddScoped<IIntentHandler, UpdateIntentHandler>();

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
    IntentRouter intentRouter,
    AssistantIntentValidator validator,
    CancellationToken cancellationToken) =>
{
    var result = await aiService.ProcessAsync(request.Input);

    var validation = validator.Validate(result.ParsedResult);

    if (!validation.IsValid)
    {
        return Results.BadRequest(new
        {
            success = false,
            error = validation.ErrorMessage
        });
    }

    var handled = await intentRouter.RouteAsync(result.ParsedResult, cancellationToken);

    return Results.Ok(new
    {
        success = true,
        data = result.ParsedResult,
        handled,
        receivedAt = DateTimeOffset.UtcNow
    });
});

app.MapGet("/tasks", async (
    ITaskRepository repository,
    int page = 1,
    int pageSize = 10,
    string? status = null,
    DateTimeOffset? dueDate = null,
    CancellationToken cancellationToken = default) =>
{
    page = Math.Max(page, 1);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var tasks = await repository.GetAllAsync(
        page,
        pageSize,
        status,
        dueDate,
        cancellationToken);

    return Results.Ok(tasks);
});

app.MapGet("/notes", async (
    INoteRepository repository,
    int page = 1,
    int pageSize = 10,
    CancellationToken cancellationToken = default) =>
{
    page = Math.Max(page, 1);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var notes = await repository.GetAllAsync(page, pageSize, cancellationToken);

    return Results.Ok(notes);
});

app.MapGet("/events", async (
    IEventRepository repository,
    int page = 1,
    int pageSize = 10,
    DateTimeOffset? from = null,
    DateTimeOffset? to = null,
    CancellationToken cancellationToken = default) =>
{
    page = Math.Max(page, 1);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var events = await repository.GetAllAsync(
        page,
        pageSize,
        from,
        to,
        cancellationToken);

    return Results.Ok(events);
});

app.MapGet("/tasks/{id:guid}", async (
    Guid id,
    ITaskRepository repository,
    CancellationToken cancellationToken) =>
{
    var task = await repository.GetByIdAsync(id, cancellationToken);

    return task is null
        ? Results.NotFound()
        : Results.Ok(task);
});

app.MapGet("/notes/{id:guid}", async (
    Guid id,
    INoteRepository repository,
    CancellationToken cancellationToken) =>
{
    var note = await repository.GetByIdAsync(id, cancellationToken);

    return note is null
        ? Results.NotFound()
        : Results.Ok(note);
});

app.MapGet("/events/{id:guid}", async (
    Guid id,
    IEventRepository repository,
    CancellationToken cancellationToken) =>
{
    var calendarEvent = await repository.GetByIdAsync(id, cancellationToken);

    return calendarEvent is null
        ? Results.NotFound()
        : Results.Ok(calendarEvent);
});

app.MapPatch("/tasks/{id:guid}", async (
    Guid id,
    UpdateTaskRequest request,
    ITaskRepository repository,
    CancellationToken cancellationToken) =>
{
    var task = await repository.GetByIdAsync(id, cancellationToken);

    if (task is null)
    {
        return Results.NotFound();
    }

    task.Update(
    request.Title,
    request.Details,
    request.DueAt,
    request.Status);

    await repository.UpdateAsync(task, cancellationToken);

    return Results.Ok(task);
});

app.MapPatch("/notes/{id:guid}", async (
    Guid id,
    UpdateNoteRequest request,
    INoteRepository repository,
    CancellationToken cancellationToken) =>
{
    var note = await repository.GetByIdAsync(id, cancellationToken);

    if (note is null)
    {
        return Results.NotFound();
    }

    note.Update(
    request.Title,
    request.Details);

    await repository.UpdateAsync(note, cancellationToken);

    return Results.Ok(note);
});

app.MapPatch("/events/{id:guid}", async (
    Guid id,
    UpdateEventRequest request,
    IEventRepository repository,
    CancellationToken cancellationToken) =>
{
    var calendarEvent = await repository.GetByIdAsync(id, cancellationToken);

    if (calendarEvent is null)
    {
        return Results.NotFound();
    }

    calendarEvent.Update(
     request.Title,
     request.Details,
     request.StartAtUtc,
     request.EndAtUtc);

    await repository.UpdateAsync(calendarEvent, cancellationToken);

    return Results.Ok(calendarEvent);
});

app.MapDelete("/tasks/{id:guid}", async (
    Guid id,
    ITaskRepository repository,
    CancellationToken cancellationToken) =>
{
    var task = await repository.GetByIdAsync(id, cancellationToken);

    if (task is null)
    {
        return Results.NotFound();
    }

    await repository.DeleteAsync(task, cancellationToken);

    return Results.NoContent();
});

app.MapDelete("/notes/{id:guid}", async (
    Guid id,
    INoteRepository repository,
    CancellationToken cancellationToken) =>
{
    var note = await repository.GetByIdAsync(id, cancellationToken);

    if (note is null)
    {
        return Results.NotFound();
    }

    await repository.DeleteAsync(note, cancellationToken);

    return Results.NoContent();
});

app.MapDelete("/events/{id:guid}", async (
    Guid id,
    IEventRepository repository,
    CancellationToken cancellationToken) =>
{
    var calendarEvent = await repository.GetByIdAsync(id, cancellationToken);

    if (calendarEvent is null)
    {
        return Results.NotFound();
    }

    await repository.DeleteAsync(calendarEvent, cancellationToken);

    return Results.NoContent();
});

app.Run();

public sealed record UpdateTaskRequest(
    string? Title,
    string? Details,
    DateTimeOffset? DueAt,
    string? Status);

public sealed record UpdateNoteRequest(
    string? Title,
    string? Details);

public sealed record UpdateEventRequest(
    string? Title,
    string? Details,
    DateTimeOffset? StartAtUtc,
    DateTimeOffset? EndAtUtc);
