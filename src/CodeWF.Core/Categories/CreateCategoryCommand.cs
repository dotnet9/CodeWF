using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using CodeWF.Core.Abouts;
using CodeWF.Data;
using Microsoft.Extensions.Caching.Memory;

namespace CodeWF.Core.Categories;

public class CreateCategoryCommand : IRequest
{
    [Required]
    [Display(Name = "Name")]
    [MaxLength(64)]
    public string Name { get; set; }

    [Required]
    [Display(Name = "Slug")]
    [RegularExpression("(?!-)([a-z0-9-]+)")]
    [MaxLength(64)]
    public string Slug { get; set; }

    public int Sort { get; set; }
}

public class CreateCategoryCommandHandler(
    CodeWFRepository<Category> repository,
    IMemoryCache cache,
    ILogger<GetAboutQueryHandler> logger) : IRequestHandler<CreateCategoryCommand>
{
    public async Task Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var exists = await repository.AnyAsync(new CategoryBySlugSpec(request.Slug), ct);
        if (exists) return;

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug.Trim(),
            Name = request.Name.Trim(),
            Sort = request.Sort,
        };

        await repository.AddAsync(category, ct);
        cache.Remove(CacheKeys.CategoryList);

        logger.LogInformation("Category created: {Category}", category.Id);
    }
}