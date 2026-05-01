using System.Text.RegularExpressions;
using Markdig;

namespace WebApp.Extensions;

public static class MarkdownExtension
{
    public static string? ToHtml(this string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return default;

        // 统一在这里配置 Markdown 管线，保证文章页、文档页和 about/donation 的渲染行为一致。
        var pipelineBuilder = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseBootstrap();
        pipelineBuilder.UseAdvancedExtensions();
        var pipeline = pipelineBuilder.Build();

        var html = Markdown.ToHtml(markdown, pipeline);

        html = AddCodeBlockLanguageClass(html);
        html = AddImagePerformanceAttributes(html);

        return html;
    }

    private static string AddCodeBlockLanguageClass(string html)
    {
        var codeBlockPattern = new Regex(@"<pre><code(?:\s+class=""([^""]*)"")?>([\s\S]*?)</code></pre>", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        return codeBlockPattern.Replace(html, match =>
        {
            var existingClass = match.Groups[1].Value;
            var codeContent = match.Groups[2].Value;

            var languageClass = DetermineLanguageClass(codeContent, existingClass);

            return $"<pre><code class=\"{languageClass}\">{codeContent}</code></pre>";
        });
    }

    private static string DetermineLanguageClass(string codeContent, string existingClass)
    {
        // 历史文章里并不是每个代码块都补了语言标签，这里用启发式规则兜底高亮体验。
        if (!string.IsNullOrEmpty(existingClass) && !existingClass.Contains("language-"))
        {
            return $"language-{existingClass}";
        }

        if (!string.IsNullOrEmpty(existingClass))
        {
            return existingClass;
        }

        codeContent = codeContent.TrimStart();

        if (codeContent.StartsWith("using ") || codeContent.StartsWith("namespace ") ||
            codeContent.Contains(" class ") || codeContent.Contains("public ") ||
            codeContent.Contains("private ") || codeContent.Contains("protected "))
        {
            return "language-csharp";
        }

        if (codeContent.StartsWith("{") || codeContent.StartsWith("["))
        {
            return "language-json";
        }

        if (codeContent.StartsWith("#") || codeContent.Contains("---"))
        {
            return "language-yaml";
        }

        if (codeContent.StartsWith("<") && codeContent.Contains(">"))
        {
            return "language-markup";
        }

        if (codeContent.StartsWith("SELECT ") || codeContent.StartsWith("INSERT ") ||
            codeContent.StartsWith("UPDATE ") || codeContent.StartsWith("DELETE ") ||
            codeContent.StartsWith("CREATE ") || codeContent.Contains(" FROM "))
        {
            return "language-sql";
        }

        if (codeContent.StartsWith("import ") || codeContent.StartsWith("export ") ||
            codeContent.Contains("function ") || codeContent.Contains("const ") ||
            codeContent.Contains("let ") || codeContent.Contains("=>"))
        {
            return "language-javascript";
        }

        if (codeContent.Contains("@") && codeContent.Contains("page"))
        {
            return "language-aspnet";
        }

        if (codeContent.Contains("SELECT") || codeContent.Contains("FROM"))
        {
            return "language-sql";
        }

        return "language-text";
    }

    private static string AddImagePerformanceAttributes(string html)
    {
        // 站内大部分 Markdown 图片都不是首屏关键资源，默认补齐懒加载和异步解码。
        var imagePattern = new Regex(@"<img\b(?![^>]*\bloading=)(?![^>]*\bdecoding=)([^>]*?)\s*/?>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        return imagePattern.Replace(html, match =>
        {
            var attributes = match.Groups[1].Value.TrimEnd();
            return $"<img {attributes} loading=\"lazy\" decoding=\"async\" />";
        });
    }
}
