using CodeWF.WebAPI.ViewModels;

namespace CodeWF.WebAPI.Options;

/// <summary>
///     网站基本信息
/// </summary>
public class SiteInfo
{
    /// <summary>
    ///     网站名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     网站描述
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    /// 特色关键字
    /// </summary>
    public string[]? FeatureKeywords { get; set; }

    /// <summary>
    ///     网站Logo
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    ///     网站所有者
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    ///     网站所有者微信二维码
    /// </summary>
    public string? OwnerWeChat { get; set; }

    /// <summary>
    ///     网站公众号
    /// </summary>
    public List<WeChatPublic>? WeChatPublic { get; set; }

    /// <summary>
    ///     网站创建起始年份
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    ///     工具网站地址
    /// </summary>
    public string? ToolUrl { get; set; }

    /// <summary>
    ///     博文网站地址
    /// </summary>
    public string? BlogPostUrl { get; set; }
}