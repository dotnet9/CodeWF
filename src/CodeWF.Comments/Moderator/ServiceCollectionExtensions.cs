namespace CodeWF.Comments.Moderator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContentModerator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IModeratorService, AzureFunctionModeratorService>();
        return services;
    }
}