namespace CodeWF.Data.Entities;

public class ResourcePath
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Category { get; set; }

    public string? Cover { get; set; }

    public string? Url { get; set; }

    public string? Introduction { get; set; }
    public string ResourceType { get; set; } = null!;
    public EnabledKind Status { get; set; }

    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}