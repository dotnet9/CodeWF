using Known.Components;

namespace CodeWF;

public static class AppConfig
{
    public static string AppId => "CodeWF";

    public static string AppName => "码坊";

    public static void AddCodeWF(this IServiceCollection services)
    {
        Console.WriteLine(AppName);

        var assembly = typeof(AppConfig).Assembly;

        services.AddKnown(info =>
        {
            info.Id = AppId;
            info.Name = AppName;
            info.IsSize = true;
            info.IsLanguage = true;
            info.IsTheme = true;
            info.Assembly = assembly;
        });
        
        //添加样式
        KStyleSheet.AddStyle("_content/AntBlazor/css/ant-design-blazor.css");
        KStyleSheet.AddStyle("_content/AntBlazor/css/web.css");
        KStyleSheet.AddStyle("_content/CodeWF/css/editormd.css");
        KStyleSheet.AddStyle("_content/CodeWF/css/toc.css");
        KStyleSheet.AddStyle("_content/CodeWF/css/web.css");

        //添加脚本
        KScript.AddScript("_content/AntBlazor/js/AntBlazor.js");
        KScript.AddScript("_content/CodeWF/js/editormd.js");

        Config.AddModule(assembly);
    }
}