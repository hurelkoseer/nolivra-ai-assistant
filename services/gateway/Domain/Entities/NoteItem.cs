namespace Nolivra.Gateway.Domain.Entities;

public sealed class NoteItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = default!;
    public string? Details { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private NoteItem()
    {
    }

    public static NoteItem Create(string title, string? details)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        return new NoteItem
        {
            Title = title.Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim()
        };
    }

    public void Update(
    string? title,
    string? details)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }

        if (details is not null)
        {
            Details = details;
        }
    }
}
