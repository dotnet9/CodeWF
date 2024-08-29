using CodeWF.Data.Entities;
using Microsoft.Extensions.Logging;

namespace CodeWF.Data;

public class Seed
{
    private static string _assetsDir = null!;
    private const string SiteDir = "site";
    private const string AboutFileName = "about.md";
    private const string CategoryFileName = "category.json";

    public static async Task SeedAsync(BlogDbContext dbContext, string assetsDir, ILogger logger, int retry = 0)
    {
        _assetsDir = assetsDir;
        var retryForAvailability = retry;

        try
        {
            logger.LogDebug("Adding about data...");
            await dbContext.About.AddAsync(await GetAboutAsync());
            var categories = await GetCategoriesAsync();
            if (categories?.Any() == true)
            {
                await dbContext.Category.AddRangeAsync(categories);
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            if (retryForAvailability >= 10) throw;

            retryForAvailability++;
            logger.LogError(ex.Message);
            await SeedAsync(dbContext, assetsDir, logger, retryForAvailability);
            throw;
        }
    }

    private static async Task<About> GetAboutAsync()
    {
        var file = Path.Combine(_assetsDir, SiteDir, AboutFileName);
        About about = new()
        {
            Id = Guid.NewGuid(),
            Title = "About"
        };
        if (File.Exists(file))
        {
            about.Content = await File.ReadAllTextAsync(file);
        }
        else
        {
            about.Content = "Test content about website";
        }

        return about;
    }

    private static async Task<List<Category>?> GetCategoriesAsync()
    {
        var file = Path.Combine(_assetsDir, SiteDir, CategoryFileName);
        if (!File.Exists(file))
        {
            return [new Category { Id = Guid.NewGuid(), Name = ".NET", Slug = "dotnet" }];
        }

        var jsonStr = await File.ReadAllTextAsync(file);
        var categories = System.Text.Json.JsonSerializer.Deserialize<List<Category>>(jsonStr);
        if (categories?.Any() == true)
        {
            categories.ForEach(category => category.Id = Guid.NewGuid());
        }

        return categories;
    }
}