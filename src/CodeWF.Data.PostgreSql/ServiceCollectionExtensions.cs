namespace CodeWF.Data.PostgreSql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlStorage(this IServiceCollection services, string connectionString)
    {
        services.AddScoped(typeof(IRepository<>), typeof(PostgreSqlDbContextRepository<>));

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<PostgreSqlBlogDbContext>(optionsAction => optionsAction
            .UseLazyLoadingProxies()
            .EnableDetailedErrors()
            .UseNpgsql(connectionString, options =>
            {
                options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
            }));

        return services;
    }
}