using CodeWF.Data.SQLite.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWF.Data.SQLite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSQLiteStorage(this IServiceCollection services, string connectionString)
    {
        services.AddScoped(typeof(CodeWFRepository<>), typeof(SQLiteDbContextRepository<>));

        services.AddDbContext<SQLiteBlogDbContext>(optionsAction => optionsAction.UseLazyLoadingProxies()
            .UseSqlite(connectionString)
            .EnableDetailedErrors());

        return services;
    }
}