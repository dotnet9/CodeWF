using CodeWF.Data;
using CodeWF.Data.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CodeWF.Core.CategoryFeature;

public record DeleteCategoryCommand(Guid Id) : IRequest<OperationCode>;

public class DeleteCategoryCommandHandler(
    CodeWFRepository<CategoryEntity> catRepo,
    IMemoryCache cache,
    ILogger<DeleteCategoryCommandHandler> logger)
    : IRequestHandler<DeleteCategoryCommand, OperationCode>
{
    public async Task<OperationCode> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var cat = await catRepo.GetByIdAsync(request.Id, ct);
        if (null == cat) return OperationCode.ObjectNotFound;

        cat.PostCategory.Clear();

        await catRepo.DeleteAsync(cat, ct);
        cache.Remove(CacheKeys.CategoryList);

        logger.LogInformation("Category deleted: {Category}", cat.Id);
        return OperationCode.Done;
    }
}