namespace CodeWF.Data.Entities;

public class WeiYan
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int LikeCount { get; set; }

    public string Content { get; set; } = null!;

    public string Type { get; set; } = null!;

    public Guid? Source { get; set; }

    public EnabledKind IsPublic { get; set; }

    public DateTime CreateTime { get; set; }
}