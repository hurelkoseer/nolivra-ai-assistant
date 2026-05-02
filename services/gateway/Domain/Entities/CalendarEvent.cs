namespace Nolivra.Gateway.Domain.Entities;

public sealed class CalendarEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = default!;
    public string? Details { get; private set; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset? EndAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private CalendarEvent()
    {
    }

    public static CalendarEvent Create(string title, string? details, DateTimeOffset startAtUtc, DateTimeOffset? endAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (endAtUtc != null && startAtUtc >= endAtUtc)
            throw new ArgumentException("StartAt must be before EndAt.", nameof(startAtUtc));

        return new CalendarEvent
        {
            Title = title.Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            StartAtUtc = startAtUtc.ToUniversalTime(),
            EndAtUtc = endAtUtc?.ToUniversalTime()
        };
    }

    public void Update(
    string? title,
    string? details,
    DateTimeOffset? startAtUtc,
    DateTimeOffset? endAtUtc)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }

        if (details is not null)
        {
            Details = details;
        }

        if (startAtUtc.HasValue)
        {
            StartAtUtc = startAtUtc.Value;
        }

        if (endAtUtc.HasValue)
        {
            EndAtUtc = endAtUtc.Value;
        }
    }
}
