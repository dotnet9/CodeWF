using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using MediatR;

namespace CodeWF.Core.Categories;

public record GetCategoryBySlugQuery(string Slug) : IRequest<Category?>;

public class GetCategoryByRouteQueryHandler(CodeWFRepository<Category> repository)
    : IRequestHandler<GetCategoryBySlugQuery, Category?>
{
    public Task<Category?> Handle(GetCategoryBySlugQuery request, CancellationToken ct) =>
        repository.FirstOrDefaultAsync(new CategoryBySlugSpec(request.Slug), ct);
}