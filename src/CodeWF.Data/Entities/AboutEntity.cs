namespace CodeWF.Data.Entities;

public class AboutEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public DateTime UpdateTime { get; set; }
}