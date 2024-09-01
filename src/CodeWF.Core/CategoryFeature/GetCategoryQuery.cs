using CodeWF.Data;
using CodeWF.Data.Entities;
using MediatR;

namespace CodeWF.Core.CategoryFeature;

public record GetCategoryQuery(Guid Id) : IRequest<CategoryEntity?>;

public class GetCategoryByIdQueryHandler(CodeWFRepository<CategoryEntity> repo)
    : IRequestHandler<GetCategoryQuery, CategoryEntity?>
{
    public async Task<CategoryEntity?> Handle(GetCategoryQuery request, CancellationToken ct) =>
        await repo.GetByIdAsync(request.Id, ct);
}