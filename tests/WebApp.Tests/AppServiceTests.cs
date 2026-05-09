using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
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

    [Fact]
    public async Task SearchAsync_WritesLanguageSpecificSearchKeywordFiles()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        using var appService = CreateAppService();

        try
        {
            RequestLanguage.CurrentLanguage = "en";
            await appService.SearchAsync("dotnet ai", 1, 10);

            RequestLanguage.CurrentLanguage = "ja";
            await appService.SearchAsync("avalonia ui", 1, 10);

            var enPath = Path.Combine(_tempRoot, "site", "search-keywords.en.json");
            var jaPath = Path.Combine(_tempRoot, "site", "search-keywords.ja.json");

            Assert.True(File.Exists(enPath));
            Assert.True(File.Exists(jaPath));
            Assert.False(File.Exists(Path.Combine(_tempRoot, "site", "search-keywords.json")));
            AssertSearchKeywordFileContains(enPath, "dotnet ai");
            AssertSearchKeywordFileDoesNotContain(enPath, "avalonia ui");
            AssertSearchKeywordFileContains(jaPath, "avalonia ui");
            AssertSearchKeywordFileDoesNotContain(jaPath, "dotnet ai");
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task ReadAboutAsync_CreatesLanguageSidecar_WhenMissing()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "about.md"), "# 关于\n\n中文内容。");

        var appService = CreateAppService(new FakeContentTranslationService("# About\n\nTranslated content."));
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var (markdown, htmlContent) = await appService.ReadAboutAsync();
            var translatedPath = Path.Combine(_tempRoot, "site", "about.en.md");

            Assert.True(File.Exists(translatedPath));
            Assert.Contains("Translated content.", markdown);
            Assert.Contains("Translated content.", htmlContent);
        }
        finally
        {
            RequestLanguage.Clear();
            appService.Dispose();
        }
    }

    [Fact]
    public async Task GetPostBySlug_CreatesVersionedTranslation_AndDeletesStaleVersion()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        Directory.CreateDirectory(postDir);
        var sourcePath = Path.Combine(postDir, "sample-post.md");
        var stalePath = Path.Combine(postDir, "sample-post.20260501213214.en.md");
        var expectedPath = Path.Combine(postDir, "sample-post.20260501213231.en.md");

        await File.WriteAllTextAsync(sourcePath, """
            ---
            title: 中文标题
            slug: sample-post
            description: 中文描述
            date: 2026-05-01 10:30:00
            lastmod: 2026-05-01 21:32:31
            categories:
              - Web开发
            tags:
              - 网站重构
            draft: false
            ---

            中文正文。
            """);

        await File.WriteAllTextAsync(stalePath, """
            ---
            title: Old title
            slug: sample-post
            description: Old description
            date: 2026-05-01 10:30:00
            lastmod: 2026-05-01 21:32:14
            draft: false
            ---

            Old body.
            """);

        var translatedMarkdown = """
            ---
            title: English title
            slug: sample-post
            description: English description
            date: 2026-05-01 10:30:00
            lastmod: 2026-05-01 21:32:31
            categories:
              - Web Development
            tags:
              - Website Rebuild
            draft: false
            ---

            English body.
            """;

        var appService = CreateAppService(new FakeContentTranslationService(translatedMarkdown));
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var post = await appService.GetPostBySlug("sample-post");

            Assert.NotNull(post);
            Assert.Equal("English title", post.Title);
            Assert.Equal("sample-post", post.Slug);
            Assert.Contains("Web Development", post.Categories ?? []);
            Assert.Contains("Website Rebuild", post.Tags ?? []);
            Assert.True(File.Exists(expectedPath));
            Assert.False(File.Exists(stalePath));
            var translatedFile = await File.ReadAllTextAsync(expectedPath);
            Assert.Contains("  - Web Development", translatedFile);
            Assert.Contains("  - Website Rebuild", translatedFile);
        }
        finally
        {
            RequestLanguage.Clear();
            appService.Dispose();
        }
    }

    [Fact]
    public async Task GetPostByAlbum_LoadsPostsFromAllYearDirectories()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "albums.json"), """
            [
              {
                "Sort": 1,
                "Name": "Blazor组件库",
                "Memo": "Blazor组件库文章",
                "Slug": "blazor-component-library"
              }
            ]
            """);

        var postDir = Path.Combine(_tempRoot, "2021", "12");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "old-blazor-post.md"), """
            ---
            title: 老文章
            slug: old-blazor-post
            description: 旧年份文章
            date: 2021-12-01 10:00:00
            albums:
              - Blazor组件库
            draft: false
            ---

            正文。
            """);

        using var appService = CreateAppService();

        var page = await appService.GetPostByAlbum(1, 10, "blazor-component-library", null);

        Assert.Equal(1, page.Total);
        Assert.Equal("old-blazor-post", page.Data.Single().Slug);
    }

    [Fact]
    public async Task GetAllBlogPostsAsync_ReadsMetadataWithoutRenderingBodyHtml()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "metadata-only.md"), """
            ---
            title: 元数据文章
            slug: metadata-only
            description: 用于验证列表读取
            date: 2026-05-01 10:00:00
            draft: false
            ---

            # 正文标题

            正文内容。
            """);

        using var appService = CreateAppService();

        var posts = await appService.GetAllBlogPostsAsync();
        var post = Assert.Single(posts ?? []);

        Assert.Equal("metadata-only", post.Slug);
        Assert.Null(post.Content);
        Assert.Null(post.HtmlContent);
    }

    [Fact]
    public async Task GetPostByCategory_FiltersByDecodedCategoryName_WhenCategoryIsNotConfigured()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "ai-post.md"), """
            ---
            title: AI文章
            slug: ai-post
            description: AI分类文章
            date: 2026-05-01 10:00:00
            categories:
              - Web开发
            draft: false
            ---

            正文。
            """);

        using var appService = CreateAppService();

        var page = await appService.GetPostByCategory(1, 10, "Web%E5%BC%80%E5%8F%91", null);

        Assert.Equal(1, page.Total);
        Assert.Equal("ai-post", page.Data.Single().Slug);
    }

    [Fact]
    public void LocalizePath_DoesNotTreatCategoryRouteSegmentAsLanguage()
    {
        RequestLanguage.CurrentLanguage = "zh-cn";

        try
        {
            Assert.Equal("/zh-cn/cat/dotnet", RequestLanguage.LocalizePath("/cat/dotnet"));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public void I18nService_CreatesMissingLanguageResource_FromDefaultResource()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site", "i18n"));
        File.WriteAllText(Path.Combine(_tempRoot, "site", "i18n", "zh-cn.json"), """
            {
              "strings": {
                "nav.home": "首页"
              },
              "textMap": {
                "复制": "复制"
              },
              "patterns": []
            }
            """);

        var translatedJson = """
            {
              "strings": {
                "nav.home": "Home"
              },
              "textMap": {
                "复制": "Copy"
              },
              "patterns": []
            }
            """;

        var service = CreateI18nService(new FakeContentTranslationService(translatedJson));
        RequestLanguage.CurrentLanguage = "fr";

        try
        {
            var translatedPath = Path.Combine(_tempRoot, "site", "i18n", "fr.json");
            var before = service.GetLanguageResourceStatus("fr");

            Assert.False(before.HasResourceFile);
            var prepareResult = service.PrepareLanguageResource("fr");
            Assert.True(prepareResult.CreatedResourceFile);
            Assert.True(prepareResult.HasResourceFile);
            Assert.Equal("Home", service.T("nav.home", "fallback"));
            Assert.True(File.Exists(translatedPath));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public void I18nService_TranslatesDiscoveredPageText_WhenLanguageResourceIsMissing()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site", "i18n"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Pages"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Views", "Shared", "Components", "NewWidget"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "wwwroot", "js"));
        File.WriteAllText(Path.Combine(_tempRoot, "site", "i18n", "zh-cn.json"), """
            {
              "strings": {
                "nav.home": "首页"
              },
              "textMap": {},
              "patterns": []
            }
            """);
        File.WriteAllText(Path.Combine(_tempRoot, "Pages", "NewPage.cshtml"), """
            <section>
                <h1>新链接页面标题</h1>
                <p title="新链接提示">新链接页面说明</p>
            </section>
            """);
        File.WriteAllText(Path.Combine(_tempRoot, "Views", "Shared", "Components", "NewWidget", "Default.cshtml"), """
            <strong>新链接组件文字</strong>
            """);
        File.WriteAllText(Path.Combine(_tempRoot, "wwwroot", "js", "new-page.js"), """
            const message = "新链接脚本文案";
            """);

        var service = CreateI18nService(new MappingContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var translatedPath = Path.Combine(_tempRoot, "site", "i18n", "en.json");

            Assert.Equal("[en]新链接页面标题", service.Text("新链接页面标题"));
            Assert.Equal("[en]新链接提示", service.Text("新链接提示"));
            Assert.Equal("[en]新链接页面说明", service.Text("新链接页面说明"));
            Assert.Equal("[en]新链接组件文字", service.Text("新链接组件文字"));
            Assert.Equal("[en]新链接脚本文案", service.Text("新链接脚本文案"));
            Assert.True(File.Exists(translatedPath));

            using var clientResource = JsonDocument.Parse(service.GetClientResourceJson());
            Assert.Contains(clientResource.RootElement.GetProperty("languages").EnumerateArray(), item =>
                string.Equals(item.GetString(), "en", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public void I18nService_BackfillsDiscoveredPageText_WhenLanguageResourceExists()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site", "i18n"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Pages"));
        File.WriteAllText(Path.Combine(_tempRoot, "site", "i18n", "zh-cn.json"), """
            {
              "strings": {
                "nav.home": "首页"
              },
              "textMap": {},
              "patterns": []
            }
            """);
        File.WriteAllText(Path.Combine(_tempRoot, "site", "i18n", "en.json"), """
            {
              "strings": {
                "nav.home": "Home"
              },
              "textMap": {
                "已有文案": "Existing text"
              },
              "patterns": []
            }
            """);
        File.WriteAllText(Path.Combine(_tempRoot, "Pages", "AnotherPage.cshtml"), """
            <h1>后来新增页面文案</h1>
            """);

        var service = CreateI18nService(new MappingContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var translatedPath = Path.Combine(_tempRoot, "site", "i18n", "en.json");

            Assert.Equal("[en]后来新增页面文案", service.Text("后来新增页面文案"));
            Assert.Contains("后来新增页面文案", File.ReadAllText(translatedPath));
            Assert.Contains("[en]后来新增页面文案", File.ReadAllText(translatedPath));
        }
        finally
        {
            RequestLanguage.Clear();
        }
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

    private AppService CreateAppService(IContentTranslationService? translationService = null)
    {
        var siteOptions = Microsoft.Extensions.Options.Options.Create(new SiteOption
        {
            LocalAssetsDir = _tempRoot,
            StartYear = 2026
        });

        return new AppService(siteOptions, new TestWebHostEnvironment
        {
            EnvironmentName = Environments.Development,
            ContentRootPath = _tempRoot,
            WebRootPath = _tempRoot
        }, translationService);
    }

    private I18nService CreateI18nService(IContentTranslationService translationService)
    {
        var siteOptions = Microsoft.Extensions.Options.Options.Create(new SiteOption
        {
            LocalAssetsDir = _tempRoot,
            StartYear = 2026
        });

        return new I18nService(
            siteOptions,
            new TestWebHostEnvironment
            {
                EnvironmentName = Environments.Development,
                ContentRootPath = _tempRoot,
                WebRootPath = _tempRoot
            },
            new HttpContextAccessor(),
            translationService);
    }

    private static void AssertSearchKeywordFileContains(string filePath, string query)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(filePath));
        Assert.Contains(document.RootElement.EnumerateArray(), item =>
            string.Equals(GetSearchKeywordQuery(item), query, StringComparison.OrdinalIgnoreCase));
    }

    private static void AssertSearchKeywordFileDoesNotContain(string filePath, string query)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(filePath));
        Assert.DoesNotContain(document.RootElement.EnumerateArray(), item =>
            string.Equals(GetSearchKeywordQuery(item), query, StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetSearchKeywordQuery(JsonElement item)
    {
        if (item.TryGetProperty("Query", out var query)
            || item.TryGetProperty("query", out query))
        {
            return query.GetString();
        }

        return null;
    }

    private sealed class FakeContentTranslationService : IContentTranslationService
    {
        private readonly string _translated;

        public FakeContentTranslationService(string translated)
        {
            _translated = translated;
        }

        public Task<string?> TranslateAsync(
            string source,
            LanguageInfo targetLanguage,
            ContentTranslationKind kind,
            string? resourceName = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(_translated);
    }

    private sealed class MappingContentTranslationService : IContentTranslationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public Task<string?> TranslateAsync(
            string source,
            LanguageInfo targetLanguage,
            ContentTranslationKind kind,
            string? resourceName = null,
            CancellationToken cancellationToken = default)
        {
            var resource = JsonSerializer.Deserialize<I18nResource>(source, JsonOptions) ?? new I18nResource();
            foreach (var item in resource.Strings.ToList())
            {
                resource.Strings[item.Key] = $"[{targetLanguage.Code}]{item.Value}";
            }

            foreach (var item in resource.TextMap.ToList())
            {
                resource.TextMap[item.Key] = $"[{targetLanguage.Code}]{item.Key}";
            }

            return Task.FromResult<string?>(JsonSerializer.Serialize(resource));
        }
    }
}
