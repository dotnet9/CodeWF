using CodeWF.Web.Models;
using Microsoft.AspNetCore.Components;

namespace CodeWF.Web.Components;

/// <summary>文章卡片（列表/首页/专题通用）。</summary>
public sealed partial class PostCard
{
    [Parameter, EditorRequired] public BlogPostBrief Post { get; set; } = default!;

    private string Cover => string.IsNullOrWhiteSpace(Post.Cover) ? string.Empty : Post.Cover;
    private string DateText => (Post.Date ?? Post.Lastmod)?.ToString("yyyy/MM/dd") ?? string.Empty;
    private string FirstCategory => Post.Categories?.FirstOrDefault() ?? string.Empty;
}
