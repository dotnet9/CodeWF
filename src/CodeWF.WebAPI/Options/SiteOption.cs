namespace CodeWF.WebAPI.Options;

public class SiteOption
{
    /// <summary>
    ///     网站名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     网站创建起始年份
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    ///     资源目录，网站的静态资源路径
    /// </summary>
    public string? Assets { get; set; }
}