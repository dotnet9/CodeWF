namespace CodeWF.WebAPI;

/// <summary>
///     友情链接
/// </summary>
public class Link
{
    /// <summary>
    ///     排序，数字越小越靠前
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    ///     标题，一般为网站名称，或其他社交账号昵称
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     描述
    /// </summary>

    public string? Remark { get; set; }

    /// <summary>
    ///     链接
    /// </summary>
    public string? LinkUrl { get; set; }

    /// <summary>
    ///     Logo
    /// </summary>
    public string? Logo { get; set; }
}