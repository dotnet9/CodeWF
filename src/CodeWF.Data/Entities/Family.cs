namespace CodeWF.Data.Entities;

public class Family
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string BackgroundCover { get; set; } = null!;

    public string MaleCover { get; set; } = null!;

    public string FemaleCover { get; set; } = null!;

    public string MaleName { get; set; } = null!;

    public string FemaleName { get; set; } = null!;

    public string Timing { get; set; } = null!;

    public string? CountdownTitle { get; set; }

    public string? CountdownTime { get; set; }

    public EnabledKind Status { get; set; }

    public string? Info { get; set; }

    public int LikeCount { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }
}