namespace CodeWF.Data.Entities;

public class Resource
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string ResourceType { get; set; } = null!;

    public string Path { get; set; } = null!;

    public int? Size { get; set; }

    public string? OriginalName { get; set; }

    public string? MimeType { get; set; }
    public EnabledKind Status { get; set; }

    public string? StoreType { get; set; }

    public DateTime CreateTime { get; set; }
}