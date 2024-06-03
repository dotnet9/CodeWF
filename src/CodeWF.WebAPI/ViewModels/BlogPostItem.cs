namespace CodeWF.WebAPI.ViewModels;

/// <summary>
///     博文项
/// </summary>
public class BlogPostItem
{
    /// <summary>
    ///     博文标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     博文描述
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    ///     博文封面
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    ///     链接
    /// </summary>
    public string? Url { get; set; }
}