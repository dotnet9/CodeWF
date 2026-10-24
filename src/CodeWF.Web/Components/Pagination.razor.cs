using Microsoft.AspNetCore.Components;
using CodeWF.Web.Models;

namespace CodeWF.Web.Components;

/// <summary>静态 SSR 链接分页（query 参数 pageIndex），与原站 Pagination 组件口径一致：第 1 页不带参数。</summary>
public sealed partial class Pagination<T>
{
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    [Parameter, EditorRequired] public PagedResult<T> Result { get; set; } = default!;

    [Parameter] public string? BasePath { get; set; }

    private string Path => BasePath ?? Navigation.UriPath();

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(Result.Total / (double)Math.Max(1, Result.PageSize)));

    /// <summary>基于当前请求 query 生成翻页链接：pageIndex 覆盖，其余参数（q/keyword 等）保留。</summary>
    private string PageUrl(int page)
    {
        var uri = new Uri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        query["pageIndex"] = page.ToString();
        var pairs = query.AllKeys
            .OfType<string>()
            .Select(key => $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(query[key] ?? string.Empty)}");
        return Path + "?" + string.Join("&", pairs);
    }

    private IEnumerable<int> Pages
    {
        get
        {
            var current = Result.PageIndex;
            yield return 1;
            for (var p = Math.Max(2, current - 2); p <= Math.Min(TotalPages - 1, current + 2); p++)
            {
                yield return p;
            }
            if (TotalPages > 1) yield return TotalPages;
        }
    }
}
