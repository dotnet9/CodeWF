namespace CodeWF.Email.Client;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEmailSending(this IServiceCollection services)
    {
        services.AddHttpClient<ICodeWFEmailClient, CodeWFEmailClient>();
        return services;
    }
}