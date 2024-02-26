namespace CodeWF.Core.CategoryFeature;

public class Category
{
    public static readonly Expression<Func<CategoryEntity, Category>> EntitySelector = c => new Category
    {
        Id = c.Id, DisplayName = c.DisplayName, RouteName = c.RouteName, Note = c.Note
    };

    public Guid Id { get; set; }
    public string RouteName { get; set; }
    public string DisplayName { get; set; }
    public string Note { get; set; }
}