using CodeWF.Data.PostgreSQL;
using CodeWF.Data.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWF.Data.PostgreSQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSQLStorage(this IServiceCollection services, string connectionString)
    {
        services.AddScoped(typeof(CodeWFRepository<>), typeof(PostgreSQLDbContextRepository<>));

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<PostgreSQLBlogDbContext>(optionsAction => optionsAction.UseLazyLoadingProxies()
            .UseNpgsql(connectionString,
                options => { options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null); })
            .EnableDetailedErrors());

        return services;
    }
}