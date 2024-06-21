namespace CodeWF.Data.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}