using CodeWF.EventBus;

namespace CodeWF.Core.Categories;

public record GetCategoryBySlugResponse
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
}

public class GetCategoryBySlugQuery(string slug) : Query<GetCategoryBySlugResponse>
{
    public string Slug { get; set; } = slug;
    public override GetCategoryBySlugResponse Result { get; set; }
}