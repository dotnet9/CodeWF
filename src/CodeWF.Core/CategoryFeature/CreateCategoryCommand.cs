using CodeWF.Core.Abouts;
using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CodeWF.Core.CategoryFeature;

public class CreateCategoryCommand : IRequest
{
    [Required]
    [Display(Name = "Name")]
    [MaxLength(64)]
    public string DisplayName { get; set; }

    [Required]
    [Display(Name = "Slug")]
    [RegularExpression("(?!-)([a-z0-9-]+)")]
    [MaxLength(64)]
    public string Slug { get; set; }

    [Required]
    [Display(Name = "Description")]
    [MaxLength(128)]
    public string Note { get; set; }

    public int Sort { get; set; }
}

public class CreateCategoryCommandHandler(
    CodeWFRepository<CategoryEntity> repository,
    IMemoryCache cache,
    ILogger<GetAboutQueryHandler> logger) : IRequestHandler<CreateCategoryCommand>
{
    public async Task Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var exists = await repository.AnyAsync(new CategoryBySlugSpec(request.Slug), ct);
        if (exists) return;

        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Sort = request.Sort,
            Slug = request.Slug.Trim(),
            Note = request.Note.Trim(),
            DisplayName = request.DisplayName.Trim()
        };

        await repository.AddAsync(category, ct);
        cache.Remove(CacheKeys.CategoryList);

        logger.LogInformation("Category created: {Category}", category.Id);
    }
}