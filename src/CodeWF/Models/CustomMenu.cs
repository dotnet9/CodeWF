

namespace CodeWF.Models;

public class AppConfig
{
    private static AppInfo? _info;

    public static AppInfo Info(HttpRequest request)
    {
        if (_info != null) return _info;
        var menus = new Dictionary<MenuType, CustomMenuItem>();

        var scheme = request.Scheme;
        var currentDomain = request.Host;
        var blogDomain = scheme + "://" + "blog." + currentDomain;
        var toolsDomain = scheme + "://" + "tools." + currentDomain;
        var btoolsDomain = scheme + "://" + "btools." + currentDomain;

        menus[MenuType.Blog] = new CustomMenuItem(MenuType.Blog.Description(),
            "专注.NET技术分享，助力提升.NET应用开发技能，紧跟技术动态，一起探索.NET世界！源码地址：https://github.com/dotnet9/CodeWF", "浏览博文", blogDomain);
        menus[MenuType.VueTool] =
            new CustomMenuItem(MenuType.VueTool.Description(),
                "这是一个由Vue开发的在线工具类网站，提供数十款实用工具，涵盖代码格式化、JSON处理、编码转换等多种功能，满足开发者日常需求。简洁易用，无需安装，打开即用，助力提升开发效率。源码地址：https://github.com/CorentinTh/it-tools",
                "前往使用",
                toolsDomain);
        menus[MenuType.BlazorTool] =
            new CustomMenuItem(MenuType.BlazorTool.Description(),
                "这是一个基于Blazor框架打造的在线工具平台，汇聚了众多开发实用小工具，目前已开发或即将开发的如编码解码、数据加密等，轻量且强大，开箱即用，助力开发者提升工作效率。源码地址：https://github.com/dotnet9/CodeWF",
                "前往看看", btoolsDomain);

        return _info = new AppInfo("码界工坊", "一个热衷于互联网分享精神的网站", "沙漠尽头的狼", menus);
    }
}

public class AppInfo(string title, string memo, string author, Dictionary<MenuType, CustomMenuItem> menus)
{
    public string Title { get; } = title;
    public string Memo { get; } = memo;
    public string Author { get; } = author;
    public Dictionary<MenuType, CustomMenuItem> Menus { get; } = menus;
}

public enum MenuType
{
    [Description("博客")] Blog,
    [Description("IT Tools")] VueTool,
    [Description("Dotnet工具箱")] BlazorTool
}

public record CustomMenuItem(string Title, string Memo, string ButtonText, string Url);