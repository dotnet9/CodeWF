using CodeWF.Core.Abouts;
using CodeWF.Data;
using CodeWF.Data.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CodeWF.Core.Categories;

public record GetCategoriesQuery : IRequest<List<Category>?>;

public class GetCategoriesQueryHandler(
    CodeWFRepository<Category> repository,
    IMemoryCache cache,
    ILogger<GetAboutQueryHandler> logger) : IRequestHandler<GetCategoriesQuery, List<Category>?>
{
    public Task<List<Category>?> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        return cache.GetOrCreateAsync(CacheKeys.CategoryList, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            var list = await repository.ListAsync(ct);
            return list;
        });
    }
}