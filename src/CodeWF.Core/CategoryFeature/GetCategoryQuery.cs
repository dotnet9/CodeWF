namespace CodeWF.Core.CategoryFeature;

public record GetCategoryQuery(Guid Id) : IRequest<Category>;

public class GetCategoryByIdQueryHandler(IRepository<CategoryEntity> repo) : IRequestHandler<GetCategoryQuery, Category>
{
    public Task<Category> Handle(GetCategoryQuery request, CancellationToken ct)
    {
        return repo.FirstOrDefaultAsync(new CategorySpec(request.Id), Category.EntitySelector);
    }
}