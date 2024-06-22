using CodeWF.Data.Entities;
using Microsoft.Extensions.Logging;

namespace CodeWF.Data;

public class Seed
{
    public static async Task SeedAsync(BlogDbContext dbContext, ILogger logger, int retry = 0)
    {
        var retryForAvailability = retry;

        try
        {
            logger.LogDebug("Adding about data...");
            await dbContext.About.AddAsync(GetAbout());

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            if (retryForAvailability >= 10) throw;

            retryForAvailability++;
            logger.LogError(ex.Message);
            await SeedAsync(dbContext, logger, retryForAvailability);
            throw;
        }
    }

    private static About GetAbout() => new About() { Title = "About", Content = "Test about" };
}