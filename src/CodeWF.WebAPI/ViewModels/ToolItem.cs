namespace CodeWF.WebAPI.ViewModels;

/// <summary>
///     工具项
/// </summary>
public class ToolItem
{
    /// <summary>
    ///     工具名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     工具描述
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    ///     工具封面
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    ///     链接
    /// </summary>
    public string? Url { get; set; }
}