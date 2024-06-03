using CodeWF.WebAPI.Options;

namespace CodeWF.WebAPI.ViewModels;

public class SiteBase
{
    /// <summary>
    ///     网站基本信息
    /// </summary>
    public SiteInfo? Base { get; set; }

    /// <summary>
    ///     推荐工具
    /// </summary>
    public List<ToolItem>? Tools { get; set; }

    /// <summary>
    ///     推荐博文
    /// </summary>
    public List<BlogPostItem>? BlogPosts { get; set; }
}