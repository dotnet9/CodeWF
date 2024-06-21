namespace CodeWF.Data.Entities;

public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public CategoryKind Type { get; set; }

    public int Priority { get; set; }
}

public enum CategoryKind
{
    Nav = 0,
    Normal = 1
}