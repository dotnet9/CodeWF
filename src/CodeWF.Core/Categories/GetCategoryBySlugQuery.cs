using CodeWF.Core.Abouts;
using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using CodeWF.EventBus;
using Microsoft.Extensions.Logging;

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

[Event]
public class GetCategoryBySlugQueryHandler(CodeWFRepository<Category> repository, ILogger<GetAboutQueryHandler> logger)
{
    [EventHandler]
    public async Task Handle(GetCategoryBySlugQuery request)
    {
        var result = await repository.FirstOrDefaultAsync(new CategoryBySlugSpec(request.Slug));
        request.Result = new GetCategoryBySlugResponse()
        {
            Name = result?.Name,
            Slug = result?.Slug
        };
    }
}