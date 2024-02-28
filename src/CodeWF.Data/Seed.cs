using Microsoft.Extensions.Logging;

namespace CodeWF.Data;

public partial class Seed
{
    public static async Task SeedAsync(BlogDbContext dbContext, ILogger logger, bool auto, string? assetDir,
        int retry = 0)
    {
        if (!auto && (string.IsNullOrWhiteSpace(assetDir) || !Directory.Exists(assetDir)))
        {
            throw new Exception("Manually generate seeds, please configure asset directory");
        }

        int retryForAvailability = retry;

        try
        {
            await dbContext.LocalAccount.AddRangeAsync(GetLocalAccounts());
            await dbContext.BlogTheme.AddRangeAsync(GetThemes());
            if (auto)
            {
                await AddPostsAsync(dbContext);
                await dbContext.FriendLink.AddRangeAsync(GetFriendLinks());
                await dbContext.CustomPage.AddRangeAsync(GetPages());
            }
            else
            {
                await AddPostsAsync(dbContext, assetDir!);
                await dbContext.FriendLink.AddRangeAsync(GetFriendLinks(assetDir!));
                await dbContext.CustomPage.AddRangeAsync(GetPages(assetDir!));
            }


            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            if (retryForAvailability >= 10)
            {
                throw;
            }

            retryForAvailability++;

            logger.LogError(e.Message);
            await SeedAsync(dbContext, logger, auto, assetDir, retryForAvailability);
            throw;
        }
    }

    private static IEnumerable<LocalAccountEntity> GetLocalAccounts()
    {
        return new List<LocalAccountEntity>
        {
            new()
            {
                Id = Guid.Parse("ab78493d-7569-42d2-ae78-c2b610ada1aa"),
                Username = "admin",
                PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=",
                CreateTimeUtc = DateTime.UtcNow
            }
        };
    }

    private static IEnumerable<BlogThemeEntity> GetThemes()
    {
        return new List<BlogThemeEntity>
        {
            new()
            {
                ThemeName = "Word Blue",
                CssRules =
                    "{\"--accent-color1\": \"#2a579a\",\"--accent-color2\": \"#1a365f\",\"--accent-color3\": \"#3e6db5\"}",
                ThemeType = 0
            },
            new()
            {
                ThemeName = "Excel Green",
                CssRules =
                    "{\"--accent-color1\": \"#165331\",\"--accent-color2\": \"#0E351F\",\"--accent-color3\": \"#0E703A\"}",
                ThemeType = 0
            },
            new()
            {
                ThemeName = "PowerPoint Orange",
                CssRules =
                    "{\"--accent-color1\": \"#983B22\",\"--accent-color2\": \"#622616\",\"--accent-color3\": \"#C43E1C\"}",
                ThemeType = 0
            },
            new()
            {
                ThemeName = "OneNote Purple",
                CssRules =
                    "{\"--accent-color1\": \"#663276\",\"--accent-color2\": \"#52285E\",\"--accent-color3\": \"#7719AA\"}",
                ThemeType = 0
            },
            new()
            {
                ThemeName = "Outlook Blue",
                CssRules =
                    "{\"--accent-color1\": \"#035AA6\",\"--accent-color2\": \"#032B51\",\"--accent-color3\": \"#006CBF\"}",
                ThemeType = 0
            },
            new()
            {
                ThemeName = "China Red",
                CssRules =
                    "{\"--accent-color1\": \"#800900\",\"--accent-color2\": \"#5d120d\",\"--accent-color3\": \"#c5170a\"}",
                ThemeType = 0
            },
            new()
            {
                ThemeName = "Indian Curry",
                CssRules =
                    "{\"--accent-color1\": \"rgb(128 84 3)\",\"--accent-color2\": \"rgb(95 62 0)\",\"--accent-color3\": \"rgb(208 142 19)\"}",
                ThemeType = 0
            },
            new()
            {
                ThemeName = "Metal Blue",
                CssRules =
                    "{\"--accent-color1\": \"#4E5967\",\"--accent-color2\": \"#333942\",\"--accent-color3\": \"#6e7c8e\"}",
                ThemeType = 0
            }
        };
    }
}