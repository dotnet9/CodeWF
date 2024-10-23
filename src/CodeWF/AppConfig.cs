namespace CodeWF;

public static class AppConfig
{
    public static string AppId => "CodeWF";

    public static string AppName => "码界工坊";

    public static void AddCodeWF(this IServiceCollection services, Action<CMSLiteOption> action = null)
    {
        Console.WriteLine(AppName);

        var option = new CMSLiteOption();
        action?.Invoke(option);

        services.AddKnown(info =>
        {
            info.Id = AppId;
            info.Name = AppName;
            info.IsSize = true;
            info.IsLanguage = true;
            info.IsTheme = true;
            info.Assembly = typeof(AppConfig).Assembly;
        });


        if (option.IsSite)
        {
            services.AddScoped<IUIService, UIService>();
        }

        Config.AddModule(typeof(AppConfig).Assembly);
    }
}

public class CMSLiteOption
{
    public bool IsSite { get; set; } = true;
}