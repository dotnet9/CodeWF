namespace CodeWF.Data.Entities;

public class Comment
{
    public Guid Id { get; set; }

    public int Source { get; set; }

    public string Type { get; set; } = null!;

    public Guid ParentId { get; set; }

    public Guid UserId { get; set; }

    public Guid? FloorId { get; set; }

    public Guid? ParentUserId { get; set; }

    public int LikeCount { get; set; }

    public string Content { get; set; } = null!;
    public string? Info { get; set; }
    public DateTime CreateTime { get; set; }
}