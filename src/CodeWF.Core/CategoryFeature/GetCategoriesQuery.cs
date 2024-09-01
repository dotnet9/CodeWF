using CodeWF.Core.Abouts;
using CodeWF.Data;
using CodeWF.Data.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CodeWF.Core.CategoryFeature;

public record GetCategoriesQuery : IRequest<List<CategoryEntity>?>;

public class GetCategoriesQueryHandler(
    CodeWFRepository<CategoryEntity> repository,
    IMemoryCache cache,
    ILogger<GetAboutQueryHandler> logger) : IRequestHandler<GetCategoriesQuery, List<CategoryEntity>?>
{
    public Task<List<CategoryEntity>?> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        return cache.GetOrCreateAsync(CacheKeys.CategoryList, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            var list = await repository.ListAsync(ct);
            return list;
        });
    }
}