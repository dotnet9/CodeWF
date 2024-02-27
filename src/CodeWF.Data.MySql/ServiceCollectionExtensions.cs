namespace CodeWF.Data.MySql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMySqlStorage(this IServiceCollection services, string connectionString)
    {
        services.AddScoped(typeof(IRepository<>), typeof(MySqlDbContextRepository<>));

        services.AddDbContext<MySqlBlogDbContext>(optionsAction => optionsAction.UseLazyLoadingProxies()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), builder =>
            {
                builder.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
            })
            .EnableDetailedErrors());

        return services;
    }
}