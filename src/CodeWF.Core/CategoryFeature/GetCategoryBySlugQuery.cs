using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using MediatR;

namespace CodeWF.Core.CategoryFeature;

public record GetCategoryBySlugQuery(string Slug) : IRequest<CategoryEntity?>;

public class GetCategoryByRouteQueryHandler(CodeWFRepository<CategoryEntity> repository)
    : IRequestHandler<GetCategoryBySlugQuery, CategoryEntity?>
{
    public Task<CategoryEntity?> Handle(GetCategoryBySlugQuery request, CancellationToken ct) =>
        repository.FirstOrDefaultAsync(new CategoryBySlugSpec(request.Slug), ct);
}