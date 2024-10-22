using Markdig;

namespace CodeWF.Extensions;

public static class MarkdownExtension
{
    public static string? ToHtml(this string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return default;

        var pipelineBuilder = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseBootstrap();
        pipelineBuilder.UseAdvancedExtensions();
        var pipeline = pipelineBuilder.Build();

        var html = Markdown.ToHtml(markdown, pipeline);
        return html;
    }
}