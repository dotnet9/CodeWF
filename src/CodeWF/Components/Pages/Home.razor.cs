using CodeWF.Models;

namespace CodeWF.Components.Pages;

public partial class Home
{
    private static readonly List<MenuableTitleItem> menuableTitleItems =
    [
        new MenuableTitleItem("产品", "Modern App & Service Architecture", "#product-content"),
        new MenuableTitleItem("最新活动", "最新社区活动预告与往期回顾", "#activity-content"),
        new MenuableTitleItem("企业服务", "提供开源商业优质服务", "#service-content")
    ];

    private static readonly List<(string Major, string? Minor)> whyContent1 =
    [
        ("部署安装服务", null),
        ("线上故障修复", null),
        ("服务巡检", null),
        ("专属服务沟通群", null)
    ];

    private static readonly List<(string Major, string? Minor)> whyContent2 =
    [
        ("基础架构类", "如上云、架构升级、DevOps集成等"),
        ("外包服务类", "如应用现代化重构、物联网、电商项目等")
    ];

    private static readonly List<(string Major, string? Minor)> whyContent3 =
    [
        ("定制化培训服务", null),
        ("标准课程培训（初级、中级、高级、架构师）", null),
        ("外聘讲师培训", null)
    ];
}
