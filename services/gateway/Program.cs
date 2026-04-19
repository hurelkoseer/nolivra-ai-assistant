var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapPost("/assistant/process", (object request) =>
{
    return Results.Ok(new
    {
        message = "Request received",
        request
    });
});

app.Run();