namespace Nolivra.Gateway.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = default!;
    public string? Details { get; private set; }
    public DateTimeOffset? DueAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public string Status { get; private set; } = "Pending";

    private TaskItem()
    {
    }

    public static TaskItem Create(string title, string? details, DateTimeOffset? dueAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        return new TaskItem
        {
            Title = title.Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            DueAt = dueAt?.ToUniversalTime()
        };
    }

    public void Update(
    string? title,
    string? details,
    DateTimeOffset? dueAt,
    string? status)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }

        if (details is not null)
        {
            Details = details;
        }

        if (dueAt.HasValue)
        {
            DueAt = dueAt.Value;
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            Status = status;
        }
    }
}