namespace Nolivra.Gateway.Infrastructure.Middleware;

using Nolivra.Gateway.Domain.Entities;
using Nolivra.Gateway.Infrastructure.Repositories;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in {Path}", context.Request.Path);

            // Audit log for assistant endpoint exceptions
            if (context.Request.Path == "/assistant/process")
            {
                await LogAssistantRequestError(context, ex);
            }

            await WriteErrorResponse(context, ex);
        }
    }

    private static async Task LogAssistantRequestError(HttpContext context, Exception ex)
    {
        try
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body);
            var rawInput = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            var logRepository = context.RequestServices.GetRequiredService<IAssistantRequestLogRepository>();
            var log = new AssistantRequestLog
            {
                Id = Guid.NewGuid(),
                RawUserInput = rawInput,
                RawAiResponse = string.Empty,
                ParsedIntent = null,
                Success = false,
                ErrorDetail = ex.Message,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            await logRepository.SaveAsync(log, CancellationToken.None);
        }
        catch (Exception logEx)
        {
            context.RequestServices
                .GetRequiredService<ILogger<GlobalExceptionHandlingMiddleware>>()
                .LogError(logEx, "Failed to write audit log for assistant request exception");
        }
    }

    private static Task WriteErrorResponse(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var isDevelopment = context.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment();

        var detail = isDevelopment
            ? exception.Message
            : "An error occurred processing your request.";

        var problemDetails = new
        {
            type = "https://nolivra.dev/errors/internal-server-error",
            title = "Internal Server Error",
            status = context.Response.StatusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
