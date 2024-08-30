using CodeWF.Core.Abouts;
using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using CodeWF.EventBus;
using Microsoft.Extensions.Logging;

namespace CodeWF.Core.Categories;

[Event]
public class CategoryQueryHandler(CodeWFRepository<Category> repository, ILogger<GetAboutQueryHandler> logger)
{
    [EventHandler]
    public async Task GetHandleAsync(GetCategoriesQuery request)
    {
        request.Result = await repository.ListAsync(cancellationToken: default);
    }

    [EventHandler]
    public async Task GetBySlugHandleAsync(GetCategoryBySlugQuery request)
    {
        var result = await repository.FirstOrDefaultAsync(new CategoryBySlugSpec(request.Slug));
        request.Result = new GetCategoryBySlugResponse()
        {
            Name = result?.Name,
            Slug = result?.Slug
        };
    }
}