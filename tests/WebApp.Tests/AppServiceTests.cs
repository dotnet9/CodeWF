using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WebApp.Options;
using WebApp.Services;

namespace WebApp.Tests;

public sealed class AppServiceTests : IDisposable
{
    // 每个测试用独立临时目录，避免资源文件相互污染，也方便最终统一清理。
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "CodeWF.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ReadBlogPostAsync_ParsesFrontMatter_AndBuildsHtml()
    {
        Directory.CreateDirectory(_tempRoot);
        var markdownFilePath = Path.Combine(_tempRoot, "sample.md");
        await File.WriteAllTextAsync(markdownFilePath, """
            ---
            title: 测试文章
            slug: sample-post
            description: 一篇用于测试的文章
            date: 2026-05-01 12:00:00
            categories:
              - .NET
            tags:
              - AI
            ---
            
            # 标题
            
            正文内容。
            """);

        var post = await AppService.ReadBlogPostAsync(markdownFilePath);

        Assert.Equal("测试文章", post.Title);
        Assert.Equal("sample-post", post.Slug);
        Assert.Equal("一篇用于测试的文章", post.Description);
        Assert.Contains(".NET", post.Categories ?? []);
        Assert.Contains("AI", post.Tags ?? []);
        Assert.Contains("# 标题", post.Content);
        Assert.Contains("正文内容。", post.Content);
        Assert.Contains("<h1", post.HtmlContent);
        Assert.Contains("正文内容", post.HtmlContent);
    }

    [Fact]
    public async Task ReadBlogPostAsync_Throws_WhenFrontMatterEndMarkerIsMissing()
    {
        Directory.CreateDirectory(_tempRoot);
        var markdownFilePath = Path.Combine(_tempRoot, "invalid.md");
        await File.WriteAllTextAsync(markdownFilePath, """
            ---
            title: 缺少结束分隔符
            slug: invalid-post
            
            正文直接开始
            """);

        var action = () => AppService.ReadBlogPostAsync(markdownFilePath);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("No ending '---' found", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_ReturnsBlockedNotice_WhenQueryMatchesBlockedKeyword()
    {
        Directory.CreateDirectory(_tempRoot);
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        await File.WriteAllTextAsync(
            Path.Combine(_tempRoot, "site", "blocked-search-keywords.json"),
            """
            [
              {
                "Sort": 1,
                "Name": "test",
                "Memo": "blocked",
                "Keywords": ["  bad keyword  "]
              }
            ]
            """);

        var siteOptions = Microsoft.Extensions.Options.Options.Create(new SiteOption
        {
            LocalAssetsDir = _tempRoot,
            StartYear = 2026
        });

        using var appService = new AppService(siteOptions, new TestWebHostEnvironment
        {
            // 测试环境使用 Development，便于覆盖本地资源目录和内容监听相关分支。
            EnvironmentName = Environments.Development,
            ContentRootPath = _tempRoot,
            WebRootPath = _tempRoot
        });

        var result = await appService.SearchAsync("bad   keyword", 1, 10);

        Assert.True(result.IsBlocked);
        Assert.Equal("这个搜索词不适合展示，请换一个技术关键词。", result.Notice);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Total);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "WebApp.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
