using Markdig;

namespace CodeWF;

public class CmsUtils
{
    public static MarkupString GetMarkdownHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return new MarkupString("");

        var pipelineBuilder = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseBootstrap();
        pipelineBuilder.UseAdvancedExtensions();
        var pipeline = pipelineBuilder.Build();

        var html = Markdig.Markdown.ToHtml(markdown, pipeline);
        return new MarkupString(html);
    }
}