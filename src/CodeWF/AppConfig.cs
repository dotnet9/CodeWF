namespace CodeWF;

public static class AppConfig
{
    public static string AppId => "CodeWF";

    public static string AppName => "码界工坊";

    public static void AddCodeWF(this IServiceCollection services)
    {
        Console.WriteLine(AppName);

        services.AddKnown(info =>
        {
            info.Id = AppId;
            info.Name = AppName;
            info.IsSize = true;
            info.IsLanguage = true;
            info.IsTheme = true;
            info.Assembly = typeof(AppConfig).Assembly;
        });

        Config.AddModule(typeof(AppConfig).Assembly);
    }
}