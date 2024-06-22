namespace CodeWF.Data.Entities;

public class About
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public DateTime UpdateTime { get; set; }
}