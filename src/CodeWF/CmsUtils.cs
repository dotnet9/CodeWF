namespace CodeWF;

/// <summary>
/// 内容系统效用类。
/// </summary>
public class CmsUtils
{
    /// <summary>
    /// 将Markdown文本转换成HTML文本。
    /// </summary>
    /// <param name="markdown">Markdown文本。</param>
    /// <returns>HTML文本。</returns>
    public static MarkupString GetMarkdownHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return new MarkupString("");

        var html = Markdig.Markdown.ToHtml(markdown);
        return new MarkupString(html);
    }
}