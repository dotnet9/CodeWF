namespace CodeWF.Models;

public class AppConfig
{
    private static AppMemo? _info;

    public static AppMemo Info(HttpRequest request)
    {
        if (_info != null) return _info;
        var menus = new Dictionary<MenuType, CustomMenuItem>();

        var scheme = request.Scheme;
        var currentDomain = request.Host;
        var blogDomain = scheme + "://" + "blog." + currentDomain;
        var toolsDomain = scheme + "://" + "tools." + currentDomain;
        var btoolsDomain = scheme + "://" + "btools." + currentDomain;

        menus[MenuType.Blog] = new CustomMenuItem(AppInfo.AppInfo.BlogName,
            AppInfo.AppInfo.BlogDescription, "浏览博文", blogDomain);
        menus[MenuType.VueTool] =
            new CustomMenuItem("IT Tools",
                "这是一个由Vue开发的在线工具类网站，提供数十款实用工具，涵盖代码格式化、JSON处理、编码转换等多种功能，满足开发者日常需求。简洁易用，无需安装，打开即用，助力提升开发效率。源码地址：https://github.com/CorentinTh/it-tools",
                "前往使用",
                toolsDomain);
        menus[MenuType.BlazorTool] =
            new CustomMenuItem(AppInfo.AppInfo.ToolName,
                AppInfo.AppInfo.ToolDescription,
                "前往看看", btoolsDomain);

        return _info = new AppMemo(AppInfo.AppInfo.BaseName, AppInfo.AppInfo.BaseDescription, AppInfo.AppInfo.Author,
            currentDomain.ToString(), menus);
    }
}

public class AppMemo(
    string title,
    string memo,
    string author,
    string domain,
    Dictionary<MenuType, CustomMenuItem> menus)
{
    public string Title { get; } = title;
    public string Memo { get; } = memo;
    public string Author { get; } = author;
    public string Domain { get; } = domain;
    public Dictionary<MenuType, CustomMenuItem> Menus { get; } = menus;
}

public enum MenuType
{
    [Description(AppInfo.AppInfo.BlogName)]
    Blog,
    [Description("IT Tools")] VueTool,

    [Description(AppInfo.AppInfo.ToolName)]
    BlazorTool
}

public record CustomMenuItem(string Title, string Memo, string ButtonText, string Url);