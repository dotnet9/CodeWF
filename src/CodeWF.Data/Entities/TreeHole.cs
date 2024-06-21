namespace CodeWF.Data.Entities;

/// <summary>
/// 树洞
/// </summary>
public class TreeHole
{
    public Guid Id { get; set; }
    public string? Avatar { get; set; }
    public string Message { get; set; } = null!;

    public DateTime CreateTime { get; set; }
}