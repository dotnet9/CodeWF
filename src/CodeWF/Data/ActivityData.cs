using CodeWF.Models;

namespace CodeWF.Data;

public static class ActivityData
{
    public static readonly List<Activity> AllActivities = new()
    {
        new Activity(
            "NetBeauty2：让你的.NET项目输出目录更清爽",
            "在.NET项目开发中，随着项目复杂性的增加，依赖的dll文件也会逐渐增多。这往往导致输出目录混乱，不便于管理和部署。而NetBeauty2开源项目正是为了解决这一问题而生，它能够帮助开发者在独立发布.NET项目时，将.NET运行时和依赖的dll文件移动到指定的目录，从而让输出目录更加干净、清爽。",
            "https://img1.dotnet9.com/site/tools/clear.png",
            "https://img1.dotnet9.com/site/tools/clear.png",
            new DateTime(2024, 3, 12, 14, 0, 0),
            2,
            ActivityProduct.Framework,
            ActivityType.Training,
            ActivityMode.Online,
            "training-launching"),
        new Activity(
            ".NET跨平台客户端框架 - Avalonia UI",
            "这是一个基于WPF XAML的跨平台UI框架，并支持多种操作系统（Windows（.NET Core），Linux（GTK），MacOS，Android和iOS），Web（WebAssembly）",
            "https://img1.dotnet9.com/site/tools/avalonia-white-purple.svg",
            "https://img1.dotnet9.com/site/tools/avalonia-white-purple.svg",
            new DateTime(2022, 11, 19, 14, 0, 0),
            4,
            ActivityProduct.Stack,
            ActivityType.ProductLaunching,
            ActivityMode.Online,
            "v1-launching"),
        new Activity(
            "各版本操作系统对.NET支持情况",
            "MASA技术团队来深圳啦借助虚拟机和测试机，检测各版本操作系统对.NET的支持情况。安装操作系统后，实测安装相应运行时并能够运行星尘代理为通过。",
            "https://img1.dotnet9.com/site/tools/dotnet.png",
            "https://img1.dotnet9.com/site/tools/dotnet.png",
            new DateTime(2024, 1, 13, 13, 30, 0),
            4,
            ActivityProduct.None,
            ActivityType.Meetup,
            ActivityMode.Offline,
            "meetup-230318"),
        new Activity(
            ".NET反编译、第三方库调试（拦截、篡改、伪造）、一库多版本兼容",
            "使用者本人对于传播和利用本公众号提供的信息所造成的任何直接或间接的后果和损失负全部责任。公众号及作者对于这些后果不承担任何责任。如果造成后果，请自行承担责任。谢谢！",
            "https://img1.dotnet9.com/site/tools/safe.png",
            "https://img1.dotnet9.com/site/tools/safe.png",
            new DateTime(2023, 9, 26, 13, 30, 0),
            4,
            ActivityProduct.None,
            ActivityType.Meetup,
            ActivityMode.Offline,
            "meetup-230415"),
    };

    public static readonly Dictionary<string, ActivityDetail> AllActivityDetails = new()
    {
        {
            "training-launching",
            new Models.ActivityDetail("//player.bilibili.com/player.html?aid=343814267&bvid=BV1h94y1D7tw&cid=783315594&page=1&high_quality=1&autoplay=0",
                "https://cdn.masastack.com/images/m_activity22.jpg",
                "https://cdn.masastack.com/images/m_activity22.jpg",
                "https://cdn.masastack.com/files/1.%20MASA%20Framework%E7%9A%84%E8%AE%BE%E8%AE%A1%E7%90%86%E5%BF%B5.pdf")
        },
        {
            "v1-launching",
            new Models.ActivityDetail("//player.bilibili.com/player.html?aid=392957693&bvid=BV1pd4y1V7qh&cid=969406701&page=1&autoplay=0",
                "https://cdn.masastack.com/images/activity_detail_1.0.svg",
                "https://cdn.masastack.com/images/m_activity_detail_1.0.svg",
                null)
        },
        {
            "meetup-230318",
            new Models.ActivityDetail("//player.bilibili.com/player.html?aid=993829754&bvid=BV1Cx4y1w7kU&cid=1062779954&page=1&autoplay=0",
                "https://cdn.masastack.com/images/activity_detail_sz_230318.png",
                "https://cdn.masastack.com/images/m_activity_detail_sz_230318.png",
                null)
        },
        {
            "meetup-230415",
            new Models.ActivityDetail("//player.bilibili.com/player.html?aid=612597471&bvid=BV1Dh4y1W7rZ&cid=1099972463&page=1&autoplay=0",
                "https://cdn.masastack.com/images/activity_detail_cd_230415.png",
                "https://cdn.masastack.com/images/m_activity_detail_cd_230415.png",
                null)
        },
        {
            "meetup-230520",
            new Models.ActivityDetail("//player.bilibili.com/player.html?aid=443968619&bvid=BV1RL411B7Lh&cid=1138061328&page=1&autoplay=0",
                "https://cdn.masastack.com/images/activity_detail_bj_230520.png",
                "https://cdn.masastack.com/images/m_activity_detail_bj_230520.png",
                null)
        }
    };

    public static Activity LatestActivity => AllActivities.OrderByDescending(u => u.StartAt).First();

    public static List<Activity> Latest5Activities => AllActivities.OrderByDescending(u => u.StartAt).Take(5).ToList();
}
