namespace CodeWF.WebAPI.Options;

public class SiteOption : SiteInfo
{
    /// <summary>
    ///     资源目录，网站的静态资源路径
    /// </summary>
    public string? Assets { get; set; }
}