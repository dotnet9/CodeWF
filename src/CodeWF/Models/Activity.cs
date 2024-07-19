namespace CodeWF.Models;

public class Activity
{
    public string? Id { get; set; }

    public string Title { get; set; }

    public string Subtitle { get; set; }

    public string Cover { get; set; }

    public string MobileCover { get; set; }

    public DateTime StartAt { get; set; }

    /// <summary>
    /// hour
    /// </summary>
    public double Duration { get; set; }

    public ActivityType Type { get; set; }

    public ActivityMode Mode { get; set; }
    
    public ActivityProduct Product { get; set; }

    public DateOnly Date => DateOnly.FromDateTime(StartAt);

    public TimeOnly StartTime => TimeOnly.FromDateTime(StartAt);

    public TimeOnly EndTime => TimeOnly.FromDateTime(StartAt).AddHours(Duration);

    public Activity(
        string title,
        string subtitle,
        string cover,
        string mobileCover,
        DateTime startAt,
        double duration,
        ActivityProduct product,
        ActivityType type,
        ActivityMode mode,
        string? id = null)
    {
        Title = title;
        Subtitle = subtitle;
        Cover = cover;
        MobileCover = mobileCover;
        StartAt = startAt;
        Duration = duration;
        Product = product;
        Type = type;
        Mode = mode;
        Id = id;
    }
}
