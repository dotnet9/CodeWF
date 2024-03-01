namespace CodeWF.Tools.Web.Client.Common;

public static class AddCommonService
{
    public static IServiceCollection AddCodeWFService(this IServiceCollection services)
    {
        services.AddMasaBlazor(options =>
        {
            options.ConfigureSsr(ssr =>
            {
                ssr.Left = 256;
                ssr.Top = 64;
            });
        });

        services.AddSingleton<ITranslationService, TranslationService>();
        return services;
    }
}