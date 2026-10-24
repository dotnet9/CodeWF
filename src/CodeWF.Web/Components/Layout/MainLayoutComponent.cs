using CodeWF.Web.Models;
using CodeWF.Web.Services;
using Microsoft.AspNetCore.Components;

namespace CodeWF.Web.Components.Layout;

/// <summary>
/// 站点布局：顶部导航（含全局搜索）+ 页脚友链与社区链接。
/// </summary>
public class MainLayoutComponent : LayoutComponentBase
{
    [Inject] public ContentRepository Repository { get; set; } = default!;

    public SiteInfo? Site { get; private set; }
    public IReadOnlyList<FriendLinkItem> FriendLinks { get; private set; } = [];
    public IReadOnlyList<TaxonomyItem> Categories { get; private set; } = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Site = Repository.GetSiteInfo();
            FriendLinks = await Repository.GetFriendLinksAsync("zh-CN");
            Categories = await Repository.GetCategoriesAsync("zh-CN");
        }
        catch
        {
            // 数据失败不白屏：布局降级为纯骨架（与原站容错口径一致）
        }
    }

    protected string DocSiteUrl => "https://doc.dotnet9.com";

    protected string ToolsSiteUrl => "https://tools.dotnet9.com";

    protected int CurrentYear => DateTime.Now.Year;
}
