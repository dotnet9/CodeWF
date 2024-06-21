namespace CodeWF.Data.Entities;

public class Post
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CategoryId { get; set; }

    public Guid[] TagIds { get; set; } = null!;

    public string Cover { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;
    public string? VideoUrl { get; set; }

    public int ViewCount { get; set; }

    public int LikeCount { get; set; }

    public EnabledKind ViewStatus { get; set; }

    public string? Password { get; set; }

    public string? Tips { get; set; }

    public EnabledKind RecommendStatus { get; set; }

    public EnabledKind CommentStatus { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }

    public Guid UpdateBy { get; set; }

    public EnabledKind Deleted { get; set; }
}