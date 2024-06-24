using CodeWF.Data;
using CodeWF.Data.PostgreSQL;
using CodeWF.Data.SQLite;
using Microsoft.EntityFrameworkCore;

namespace CodeWF.WebAPI;

public static class WebApplicationExtensions
{
    public static async Task InitStartUp(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var env = services.GetRequiredService<IWebHostEnvironment>();

        string dbType = app.Configuration.GetConnectionString("DatabaseType")!;
        BlogDbContext context = dbType.ToLowerInvariant() switch
        {
            "sqlite" => services.GetRequiredService<SQLiteBlogDbContext>(),
            "mysql" => services.GetRequiredService<MySqlBlogDbContext>(),
            "postgresql" => services.GetRequiredService<PostgreSQLBlogDbContext>(),
            _ => throw new ArgumentOutOfRangeException(nameof(dbType))
        };

        try
        {
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception e)
        {
            app.Logger.LogCritical(e, e.Message);

            app.MapGet("/", () => Results.Problem(
                detail: "Database connection test failed, please check your connection string and firewall settings, then RESTART CodeWF manually.",
                statusCode: 500
            ));
            await app.RunAsync();
        }

        bool isNew = !await context.About.AnyAsync();
        if (isNew)
        {
            try
            {
                await SeedDatabase(app, context);
            }
            catch (Exception e)
            {
                app.Logger.LogCritical(e, e.Message);

                app.MapGet("/", () => Results.Problem(
                    detail: "Database setup failed, please check error log, then RESTART CodeWF manually.",
                    statusCode: 500
                ));
                await app.RunAsync();
            }
        }
    }
    private static async Task SeedDatabase(WebApplication app, BlogDbContext context)
    {
        app.Logger.LogInformation("Seeding database...");

        await context.ClearAllData();
        await Seed.SeedAsync(context, app.Logger);

        app.Logger.LogInformation("Database seeding successfully.");
    }
}