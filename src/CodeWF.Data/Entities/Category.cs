namespace CodeWF.Data.Entities;

public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;
    public int Sort { get; set; }
}