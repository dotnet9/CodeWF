namespace CodeWF.Core.CategoryFeature;

public class UpdateCategoryCommand : CreateCategoryCommand, IRequest<OperationCode>
{
    public Guid Id { get; set; }
}

public class UpdateCategoryCommandHandler(IRepository<CategoryEntity> repo, ICacheAside cache)
    : IRequestHandler<UpdateCategoryCommand, OperationCode>
{
    public async Task<OperationCode> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        CategoryEntity? cat = await repo.GetAsync(request.Id, ct);
        if (cat is null)
        {
            return OperationCode.ObjectNotFound;
        }

        cat.RouteName = request.RouteName.Trim();
        cat.DisplayName = request.DisplayName.Trim();
        cat.Note = request.Note?.Trim();

        await repo.UpdateAsync(cat, ct);
        cache.Remove(BlogCachePartition.General.ToString(), "allcats");

        return OperationCode.Done;
    }
}