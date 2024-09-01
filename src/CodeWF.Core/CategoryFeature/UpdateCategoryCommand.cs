using CodeWF.Data;
using CodeWF.Data.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CodeWF.Core.CategoryFeature;

public class UpdateCategoryCommand : CreateCategoryCommand, IRequest<OperationCode>
{
    public Guid Id { get; set; }
}

public class UpdateCategoryCommandHandler(
    CodeWFRepository<CategoryEntity> repo,
    IMemoryCache cache,
    ILogger<UpdateCategoryCommandHandler> logger) : IRequestHandler<UpdateCategoryCommand, OperationCode>
{
    public async Task<OperationCode> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var cat = await repo.GetByIdAsync(request.Id, ct);
        if (cat is null) return OperationCode.ObjectNotFound;

        cat.Slug = request.Slug.Trim();
        cat.DisplayName = request.DisplayName.Trim();
        cat.Note = request.Note.Trim();

        await repo.UpdateAsync(cat, ct);
        cache.Remove(CacheKeys.CategoryList);

        logger.LogInformation("Category updated: {Category}", cat.Id);
        return OperationCode.Done;
    }
}