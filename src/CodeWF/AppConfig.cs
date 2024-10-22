namespace CodeWF;

/// <summary>
/// CodeWF前台应用配置类。
/// </summary>
public static class AppConfig
{
    /// <summary>
    /// 取得应用程序ID。
    /// </summary>
    public static string AppId => "CMSLite";

    /// <summary>
    /// 取得应用程序名称。
    /// </summary>
    public static string AppName => "内容管理系统";

    /// <summary>
    /// 添加CodeWF前台应用。
    /// </summary>
    /// <param name="services">依赖注入服务。</param>
    /// <param name="action">配置选项注入。</param>
    public static void AddCodeWF(this IServiceCollection services, Action<CMSLiteOption> action = null)
    {
        Console.WriteLine(AppName);

        var option = new CMSLiteOption();
        action?.Invoke(option);

        services.AddKnown(info =>
        {
            //项目ID、名称、类型、程序集
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

        //添加模块
        Config.AddModule(typeof(AppConfig).Assembly);
    }
}

/// <summary>
/// CMSLite配置选项类。
/// </summary>
public class CMSLiteOption
{
    /// <summary>
    /// 取得或设置是否是前台站点。
    /// </summary>
    public bool IsSite { get; set; } = true;
}