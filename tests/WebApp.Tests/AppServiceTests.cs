using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    public async Task ReadBlogPostAsync_PrefersSidecarMetadata_WhenYmlExists()
    {
        Directory.CreateDirectory(_tempRoot);
        var markdownFilePath = Path.Combine(_tempRoot, "sidecar-post.md");
        await File.WriteAllTextAsync(markdownFilePath, """
            # Body title

            Body content.
            """);
        await File.WriteAllTextAsync(Path.ChangeExtension(markdownFilePath, ".yml"), """
            title: Sidecar title
            slug: sidecar-post
            description: Metadata stored outside markdown.
            date: 2026-05-01 12:00:00
            draft: false
            """);

        var post = await AppService.ReadBlogPostAsync(markdownFilePath);

        Assert.Equal("Sidecar title", post.Title);
        Assert.Equal("sidecar-post", post.Slug);
        Assert.Contains("Body content.", post.Content);
        Assert.Contains("<h1", post.HtmlContent);
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

            var enPath = Path.Combine(_tempRoot, "i18n", "en", "site", "search-keywords.json");
            var jaPath = Path.Combine(_tempRoot, "i18n", "ja", "site", "search-keywords.json");

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
            var translatedPath = Path.Combine(_tempRoot, "i18n", "en", "site", "about.md");

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
    public async Task ReadAboutAsync_FallsBackToSource_WhenTranslationFails()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "about.md"), "# 关于\n\n中文内容。");

        var appService = CreateAppService(NullContentTranslationService.Instance);
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var (markdown, htmlContent) = await appService.ReadAboutAsync();

            Assert.Contains("中文内容。", markdown);
            Assert.Contains("中文内容", htmlContent);
            Assert.False(Directory.Exists(Path.Combine(_tempRoot, "i18n", "en")));
        }
        finally
        {
            RequestLanguage.Clear();
            appService.Dispose();
        }
    }

    [Fact]
    public async Task BaiduFanyiContentTranslationService_UsesUniversalTranslateApi_WithAppIdSign()
    {
        var handler = new CapturingHttpMessageHandler("""
            {
              "from": "zh",
              "to": "en",
              "trans_result": [
                {
                  "src": "你好",
                  "dst": "Hello"
                }
              ]
            }
            """);
        var service = new BaiduFanyiContentTranslationService(
            Microsoft.Extensions.Options.Options.Create(new BaiduFanyiOption
            {
                AppID = "app123",
                Key = "secret",
                Endpoint = "https://example.test/api/trans/vip/translate"
            }),
            new SingleClientHttpClientFactory(new HttpClient(handler)),
            NullLogger<BaiduFanyiContentTranslationService>.Instance);

        var translated = await service.TranslateAsync(
            "你好",
            RequestLanguage.GetLanguage("en"),
            ContentTranslationKind.MarkdownPage);

        Assert.Equal("Hello", translated);
        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request.Method);
        Assert.Equal("https://example.test/api/trans/vip/translate", handler.Request.RequestUri?.ToString());
        Assert.Null(handler.Request.Headers.Authorization);
        var form = ParseForm(handler.Body ?? string.Empty);
        Assert.Equal("app123", form["appid"]);
        Assert.Equal("你好", form["q"]);
        Assert.Equal("zh", form["from"]);
        Assert.Equal("en", form["to"]);
        Assert.Equal(CreateMd5($"app123你好{form["salt"]}secret"), form["sign"]);
    }

    [Fact]
    public async Task TencentFanyiContentTranslationService_UsesAppIdAndKey_BeforeLegacyAliases()
    {
        var service = new TencentFanyiContentTranslationService(
            Microsoft.Extensions.Options.Options.Create(new TencentFanyiOption
            {
                AppID = "test-tencent-secret-id",
                Key = "credential-key",
                SecretId = "your secret id",
                SecretKey = "your secret key"
            }),
            NullLogger<TencentFanyiContentTranslationService>.Instance);

        var translated = await service.TranslateAsync(
            "plain ascii",
            RequestLanguage.GetLanguage("en"),
            ContentTranslationKind.MarkdownPage);

        Assert.Equal("plain ascii", translated);
    }

    [Fact]
    public void TencentFanyiContentTranslationService_UsesSdkProtocolFormat()
    {
        var getProtocol = typeof(TencentFanyiContentTranslationService).GetMethod(
            "GetProtocol",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(getProtocol);
        Assert.Equal("https://", getProtocol.Invoke(null, [new TencentFanyiOption()]));
        Assert.Equal("http://", getProtocol.Invoke(null, [new TencentFanyiOption
        {
            Endpoint = "http://tmt.tencentcloudapi.com"
        }]));
    }

    [Fact]
    public void TencentFanyiContentTranslationService_UsesConservativeRateLimitDefaults()
    {
        var getMaxCharsPerRequest = typeof(TencentFanyiContentTranslationService).GetMethod(
            "GetMaxCharsPerRequest",
            BindingFlags.NonPublic | BindingFlags.Static);
        var getMaxRequestsPerSecond = typeof(TencentFanyiContentTranslationService).GetMethod(
            "GetMaxRequestsPerSecond",
            BindingFlags.NonPublic | BindingFlags.Static);
        var getRetryDelay = typeof(TencentFanyiContentTranslationService).GetMethod(
            "GetRetryDelay",
            BindingFlags.NonPublic | BindingFlags.Static);
        var getMemoryCacheLimit = typeof(TencentFanyiContentTranslationService).GetMethod(
            "GetMemoryCacheLimit",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(getMaxCharsPerRequest);
        Assert.NotNull(getMaxRequestsPerSecond);
        Assert.NotNull(getRetryDelay);
        Assert.NotNull(getMemoryCacheLimit);
        Assert.Equal(5999, getMaxCharsPerRequest.Invoke(null, [new TencentFanyiOption()]));
        Assert.Equal(5999, getMaxCharsPerRequest.Invoke(null, [new TencentFanyiOption
        {
            MaxCharsPerRequest = 6000
        }]));
        Assert.Equal(500, getMaxCharsPerRequest.Invoke(null, [new TencentFanyiOption
        {
            MaxCharsPerRequest = 100
        }]));
        Assert.Equal(4, getMaxRequestsPerSecond.Invoke(null, [new TencentFanyiOption()]));
        Assert.Equal(1, getMaxRequestsPerSecond.Invoke(null, [new TencentFanyiOption
        {
            MaxRequestsPerSecond = 0
        }]));
        Assert.Equal(10000, getMemoryCacheLimit.Invoke(null, [new TencentFanyiOption()]));
        Assert.Equal(0, getMemoryCacheLimit.Invoke(null, [new TencentFanyiOption
        {
            MemoryCacheLimit = 0
        }]));
        Assert.Equal(
            TimeSpan.FromSeconds(2),
            getRetryDelay.Invoke(null, [new InvalidOperationException("超过了每秒频率上限"), 0]));
        Assert.Equal(
            TimeSpan.FromMilliseconds(200),
            getRetryDelay.Invoke(null, [new InvalidOperationException("temporary"), 0]));
    }

    [Fact]
    public async Task StructuredContentTranslation_BatchesJsonResourceStrings()
    {
        var requests = new List<string>();

        Task<string?> TranslateChunkAsync(
            string source,
            LanguageInfo targetLanguage,
            string resource,
            int chunkIndex,
            int chunkCount,
            CancellationToken cancellationToken)
        {
            requests.Add(source);
            return Task.FromResult<string?>(TranslateStructuredBatchPayload(source));
        }

        var translated = await StructuredContentTranslation.TranslateAsync(
            """
            {
              "strings": {
                "home": "首页",
                "copy": "复制",
                "shortcut": "按 \\ 键复制",
                "slug": "keep-slug"
              },
              "items": [
                { "title": "标题一" },
                { "title": "标题二" }
              ]
            }
            """,
            RequestLanguage.GetLanguage("en"),
            ContentTranslationKind.JsonResource,
            "test.json",
            5999,
            TranslateChunkAsync,
            CancellationToken.None);

        Assert.Single(requests);
        Assert.Contains("000000\t首页", requests[0], StringComparison.Ordinal);
        Assert.Contains("000004\t标题二", requests[0], StringComparison.Ordinal);
        Assert.DoesNotContain("CODEWF_I18N", requests[0], StringComparison.Ordinal);
        using var document = JsonDocument.Parse(translated!);
        var root = document.RootElement;
        Assert.Equal("[en]首页", root.GetProperty("strings").GetProperty("home").GetString());
        Assert.Equal("[en]复制", root.GetProperty("strings").GetProperty("copy").GetString());
        Assert.Equal("[en]按 \\ 键复制", root.GetProperty("strings").GetProperty("shortcut").GetString());
        Assert.Equal("keep-slug", root.GetProperty("strings").GetProperty("slug").GetString());
        Assert.Equal("[en]标题一", root.GetProperty("items")[0].GetProperty("title").GetString());
        Assert.Equal("[en]标题二", root.GetProperty("items")[1].GetProperty("title").GetString());
    }

    [Fact]
    public async Task StructuredContentTranslation_BatchesMarkdownSegments()
    {
        var requests = new List<string>();

        Task<string?> TranslateChunkAsync(
            string source,
            LanguageInfo targetLanguage,
            string resource,
            int chunkIndex,
            int chunkCount,
            CancellationToken cancellationToken)
        {
            requests.Add(source);
            return Task.FromResult<string?>(TranslateStructuredBatchPayload(source));
        }

        var translated = await StructuredContentTranslation.TranslateAsync(
            """
            # 中文标题

            第一段正文。
            第二段正文。

            - 列表项

            | 名称 | 说明 |
            | --- | --- |
            | 工具 | 可用 |

            ```csharp
            Console.WriteLine("代码不要翻译");
            ```

            [中文链接](https://example.com)
            """,
            RequestLanguage.GetLanguage("en"),
            ContentTranslationKind.MarkdownPage,
            "article.md",
            5999,
            TranslateChunkAsync,
            CancellationToken.None);

        Assert.Single(requests);
        Assert.Contains("000000\t中文标题", requests[0], StringComparison.Ordinal);
        Assert.Contains("000006\t工具", requests[0], StringComparison.Ordinal);
        Assert.DoesNotContain("代码不要翻译", requests[0], StringComparison.Ordinal);
        Assert.DoesNotContain("中文链接", requests[0], StringComparison.Ordinal);
        Assert.Contains("# [en]中文标题", translated);
        Assert.Contains("[en]第一段正文。", translated);
        Assert.Contains("- [en]列表项", translated);
        Assert.Contains("| [en]工具 | [en]可用 |", translated);
        Assert.Contains("Console.WriteLine(\"代码不要翻译\");", translated);
        Assert.Contains("[中文链接](https://example.com)", translated);
    }

    [Fact]
    public async Task GetPostBySlug_CreatesVersionedTranslation_AndDeletesStaleVersion()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        var localizedPostDir = Path.Combine(_tempRoot, "i18n", "en", "2026", "05");
        Directory.CreateDirectory(postDir);
        Directory.CreateDirectory(localizedPostDir);
        var sourcePath = Path.Combine(postDir, "sample-post.md");
        var stalePath = Path.Combine(localizedPostDir, "sample-post.20260501213214.md");
        var expectedPath = Path.Combine(localizedPostDir, "sample-post.20260501213231.md");
        var expectedMetadataPath = Path.Combine(localizedPostDir, "sample-post.20260501213231.yml");

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

        var appService = CreateAppService(new ArticleSidecarTranslationService());
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
            Assert.True(File.Exists(expectedMetadataPath));
            Assert.False(File.Exists(stalePath));
            var translatedFile = await File.ReadAllTextAsync(expectedPath);
            Assert.Equal("English body.", translatedFile.Trim());
            var translatedMetadata = await File.ReadAllTextAsync(expectedMetadataPath);
            Assert.Contains("title: \"English title\"", translatedMetadata);
            Assert.Contains("  - \"Web Development\"", translatedMetadata);
            Assert.Contains("  - \"Website Rebuild\"", translatedMetadata);
        }
        finally
        {
            RequestLanguage.Clear();
            appService.Dispose();
        }
    }

    [Fact]
    public async Task GetPostBySlug_ReusesLocalizedListCache_WhenTranslationAlreadyExists()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        var localizedPostDir = Path.Combine(_tempRoot, "i18n", "en", "2026", "05");
        Directory.CreateDirectory(postDir);
        Directory.CreateDirectory(localizedPostDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "cached-post.md"), """
            ---
            title: 中文标题
            slug: cached-post
            description: 中文描述
            date: 2026-05-01 10:30:00
            lastmod: 2026-05-01 21:32:31
            banner: true
            tags:
              - 缓存
            draft: false
            ---

            中文正文。
            """);
        await File.WriteAllTextAsync(Path.Combine(localizedPostDir, "cached-post.20260501213231.md"), "English body.");
        await File.WriteAllTextAsync(Path.Combine(localizedPostDir, "cached-post.20260501213231.yml"), """
            title: English title
            slug: cached-post
            description: English description
            date: 2026-05-01 10:30:00
            lastmod: 2026-05-01 21:32:31
            banner: true
            tags:
              - Cache
            draft: false
            """);

        using var appService = CreateAppService(new ThrowingContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var postsBefore = await appService.GetAllBlogPostsAsync();
            var bannerBefore = await appService.GetBannerPostAsync();
            var tagsBefore = await appService.GetAllTagItemsAsync();

            var post = await appService.GetPostBySlug("cached-post");

            Assert.NotNull(post);
            Assert.Equal("English title", post.Title);
            Assert.Same(postsBefore, await appService.GetAllBlogPostsAsync());
            Assert.Equal(
                bannerBefore?.Select(static item => item.Title).ToList(),
                (await appService.GetBannerPostAsync())?.Select(static item => item.Title).ToList());
            Assert.Same(tagsBefore, await appService.GetAllTagItemsAsync());
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task GetPostBySlug_FallsBackToSource_WhenTranslationReturnsEmpty()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "fallback-post.md"), """
            ---
            title: 中文标题
            slug: fallback-post
            description: 中文描述
            date: 2026-05-01 10:30:00
            draft: false
            ---

            中文正文。
            """);

        using var appService = CreateAppService(new EmptyContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var post = await appService.GetPostBySlug("fallback-post");

            Assert.NotNull(post);
            Assert.Equal("中文标题", post.Title);
            Assert.Contains("中文正文。", post.Content);
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task GetPostBySlug_DoesNotRetryEmptyTranslationDuringFallbackWindow()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "retry-post.md"), """
            ---
            title: 中文标题
            slug: retry-post
            description: 中文描述
            date: 2026-05-01 10:30:00
            draft: false
            ---

            中文正文。
            """);

        var translationService = new EmptyContentTranslationService();
        using var appService = CreateAppService(translationService);
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            _ = await appService.GetPostBySlug("retry-post");
            _ = await appService.GetPostBySlug("retry-post");

            Assert.Equal(1, translationService.CallCount);
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task GetAllCategoryItemsAsync_FallsBackToSource_WhenTranslationReturnsEmpty()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "categories.json"), """
            [
              {
                "Sort": 1,
                "Name": "中文分类",
                "Memo": "中文说明",
                "Slug": "cn-category"
              }
            ]
            """);

        using var appService = CreateAppService(new EmptyContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var categories = await appService.GetAllCategoryItemsAsync();

            var category = Assert.Single(categories ?? [], static item => item.Slug == "cn-category");
            Assert.Equal("中文分类", category.Name);
            Assert.False(File.Exists(Path.Combine(_tempRoot, "i18n", "en", "site", "categories.json")));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task LocalizeBlogPostMetadataAsync_CreatesMetadataSidecar_WithoutTranslatingFullArticle()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        var localizedPostDir = Path.Combine(_tempRoot, "i18n", "en", "2026", "05");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "next-post.md"), """
            ---
            title: 下一篇标题
            slug: next-post
            description: 下一篇描述
            date: 2026-05-02 10:00:00
            categories:
              - 技术文章
            tags:
              - 翻译
            draft: false
            ---

            这里是正文，元数据本地化不应该翻译整篇正文。
            """);

        var translationService = new PrefixMetadataTranslationService();
        using var appService = CreateAppService(translationService);
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var posts = await appService.GetAllBlogPostsAsync();
            var post = Assert.Single(posts ?? []);

            var localized = await appService.LocalizeBlogPostMetadataAsync(post);

            Assert.NotNull(localized);
            Assert.Equal("[en]下一篇标题", localized.Title);
            Assert.Equal("[en]下一篇描述", localized.Description);
            Assert.Contains("[en]技术文章", localized.Categories ?? []);
            Assert.Contains("[en]翻译", localized.Tags ?? []);
            Assert.Single(translationService.Requests);
            Assert.Equal(ContentTranslationKind.JsonResource, translationService.Requests[0].Kind);
            Assert.DoesNotContain("这里是正文", translationService.Requests[0].Source, StringComparison.Ordinal);
            Assert.Single(Directory.GetFiles(localizedPostDir, "next-post.*.yml"));
            Assert.Empty(Directory.GetFiles(localizedPostDir, "next-post.*.md"));
        }
        finally
        {
            RequestLanguage.Clear();
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
    public void RequestLanguage_SupportsOnlyConfiguredLanguages_AndNormalizesAliases()
    {
        var codes = RequestLanguage.SupportedLanguages.Select(static language => language.Code).ToArray();

        Assert.Equal(new[] { "zh-cn", "zh-tw", "en", "ja" }, codes);
        Assert.Equal("zh-cn", RequestLanguage.Normalize("zh-Hans-CN"));
        Assert.Equal("zh-tw", RequestLanguage.Normalize("zh-Hant"));
        Assert.Equal("zh-tw", RequestLanguage.Normalize("zh-HK"));
        Assert.Equal("en", RequestLanguage.Normalize("en-US"));
        Assert.Equal("ja", RequestLanguage.Normalize("ja-JP"));
        Assert.Null(RequestLanguage.Normalize("fr"));
        Assert.Null(RequestLanguage.Normalize("af-za"));
    }

    [Fact]
    public async Task RequestLanguageMiddleware_DoesNotRewriteSameLanguageCookie()
    {
        var middleware = new RequestLanguageMiddleware(context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = "/en/post";
        context.Request.Headers.Cookie = $"{RequestLanguage.CookieName}=en";

        await middleware.InvokeAsync(context);

        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task RequestLanguageMiddleware_AppliesRequestCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var middleware = new RequestLanguageMiddleware(context =>
        {
            Assert.Equal("en", RequestLanguage.CurrentLanguage);
            Assert.Equal("en", CultureInfo.CurrentCulture.Name);
            Assert.Equal("en", CultureInfo.CurrentUICulture.Name);
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = "/en/post";

        try
        {
            await middleware.InvokeAsync(context);
        }
        finally
        {
            RequestLanguage.Clear();
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public async Task I18nService_CreatesMissingLanguageResource_FromDefaultResource()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "lang.json"), """
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
        RequestLanguage.CurrentLanguage = "zh-tw";

        try
        {
            var translatedPath = Path.Combine(_tempRoot, "i18n", "zh-tw", "site", "lang.json");
            var before = service.GetLanguageResourceStatus("zh-tw");

            Assert.False(before.HasResourceFile);
            var prepareResult = await service.PrepareLanguageResourceAsync("zh-tw");
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
    public async Task I18nService_TranslatesDiscoveredPageText_WhenLanguageResourceIsMissing()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Pages"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Views", "Shared", "Components", "NewWidget"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "wwwroot", "js"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "lang.json"), """
            {
              "strings": {
                "nav.home": "首页"
              },
              "textMap": {},
              "patterns": []
            }
            """);
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "Pages", "NewPage.cshtml"), """
            <section>
                <h1>新链接页面标题</h1>
                <p title="新链接提示">新链接页面说明</p>
            </section>
            """);
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "Views", "Shared", "Components", "NewWidget", "Default.cshtml"), """
            <strong>新链接组件文字</strong>
            """);
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "wwwroot", "js", "new-page.js"), """
            const message = "新链接脚本文案";
            """);

        var service = CreateI18nService(new MappingContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var translatedPath = Path.Combine(_tempRoot, "i18n", "en", "site", "lang.json");
            await service.PrepareLanguageResourceAsync("en");

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
    public async Task I18nService_BackfillsDiscoveredPageText_WhenLanguageResourceExists()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "i18n", "en", "site"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "Pages"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "lang.json"), """
            {
              "strings": {
                "nav.home": "首页"
              },
              "textMap": {},
              "patterns": []
            }
            """);
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "i18n", "en", "site", "lang.json"), """
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
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "Pages", "AnotherPage.cshtml"), """
            <h1>后来新增页面文案</h1>
            """);

        var service = CreateI18nService(new MappingContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var translatedPath = Path.Combine(_tempRoot, "i18n", "en", "site", "lang.json");
            await service.PrepareLanguageResourceAsync("en");

            Assert.Equal("[en]后来新增页面文案", service.Text("后来新增页面文案"));
            Assert.Contains("后来新增页面文案", File.ReadAllText(translatedPath));
            Assert.Contains("[en]后来新增页面文案", File.ReadAllText(translatedPath));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task I18nService_RetranslatesEnglishResourceEntries_WhenExistingFileStillContainsChinese()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "i18n", "en", "site"));
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "site", "lang.json"), """
            {
              "strings": {
                "nav.home": "首页",
                "nav.donation": "赞助"
              },
              "textMap": {
                "浏览文章": "浏览文章"
              },
              "patterns": []
            }
            """);
        await File.WriteAllTextAsync(Path.Combine(_tempRoot, "i18n", "en", "site", "lang.json"), """
            {
              "strings": {
                "nav.home": "首页",
                "nav.donation": "谣言"
              },
              "textMap": {
                "浏览文章": "浏览文章",
                "function copyToClipboard(text) { alert(\"已复制到剪贴板\"); }": "function copyToClipboard(text) { alert(\"已复制到剪贴板\"); }"
              },
              "patterns": []
            }
            """);

        var service = CreateI18nService(new MappingContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            await service.PrepareLanguageResourceAsync("en");

            Assert.Equal("[en]首页", service.T("nav.home", "fallback"));
            Assert.Equal("[en]赞助", service.T("nav.donation", "fallback"));
            Assert.Equal("[en]浏览文章", service.Text("浏览文章"));
            Assert.DoesNotContain("copyToClipboard", await File.ReadAllTextAsync(
                Path.Combine(_tempRoot, "i18n", "en", "site", "lang.json")));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public void I18nService_RetranslatesBadEnglishResource_OnDirectSynchronousAccess()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "site"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "i18n", "en", "site"));
        File.WriteAllText(Path.Combine(_tempRoot, "site", "lang.json"), """
            {
              "strings": {
                "nav.home": "首页"
              },
              "textMap": {
                "浏览文章": "浏览文章"
              },
              "patterns": []
            }
            """);
        File.WriteAllText(Path.Combine(_tempRoot, "i18n", "en", "site", "lang.json"), """
            {
              "strings": {
                "nav.home": "首页"
              },
              "textMap": {
                "浏览文章": "浏览文章"
              },
              "patterns": []
            }
            """);

        var service = CreateI18nService(new MappingContentTranslationService());
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            Assert.Equal("[en]首页", service.T("nav.home", "fallback"));
            Assert.Equal("[en]浏览文章", service.Text("浏览文章"));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task GetAllBlogPostBriefsAsync_ReadsSourceMetadataWithoutCreatingLocalizedSidecars()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "metadata-list-post.md"), """
            ---
            title: 中文标题
            slug: metadata-list-post
            description: 中文描述
            date: 2026-05-01 10:30:00
            categories:
              - Web开发
            tags:
              - 缓存
            draft: false
            ---

            中文正文。
            """);

        var translationService = new PrefixMetadataTranslationService();
        using var appService = CreateAppService(translationService);
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var brief = Assert.Single(await appService.GetAllBlogPostBriefsAsync() ?? []);

            Assert.Equal("中文标题", brief.Title);
            Assert.Equal("中文描述", brief.Description);
            Assert.Contains("Web开发", brief.Categories ?? []);
            Assert.Contains("缓存", brief.Tags ?? []);
            Assert.Empty(translationService.Requests);
            var localizedPostDir = Path.Combine(_tempRoot, "i18n", "en", "2026", "05");
            var metadataFiles = Directory.Exists(localizedPostDir)
                ? Directory.GetFiles(localizedPostDir, "metadata-list-post.*.yml")
                : [];
            Assert.Empty(metadataFiles);
            Assert.False(File.Exists(Path.Combine(_tempRoot, "i18n", "en", "2026", "05", "metadata-list-post.20260501103000.md")));
        }
        finally
        {
            RequestLanguage.Clear();
        }
    }

    [Fact]
    public async Task GetPagedBlogPostsAsync_LocalizesOnlyDisplayedMetadata()
    {
        var postDir = Path.Combine(_tempRoot, "2026", "05");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "displayed-post.md"), """
            ---
            title: 展示文章
            slug: displayed-post
            description: 展示摘要
            date: 2026-05-03 10:30:00
            categories:
              - Web开发
            draft: false
            ---

            展示正文。
            """);
        await File.WriteAllTextAsync(Path.Combine(postDir, "hidden-post.md"), """
            ---
            title: 未展示文章
            slug: hidden-post
            description: 未展示摘要
            date: 2026-05-02 10:30:00
            categories:
              - Web开发
            draft: false
            ---

            未展示正文。
            """);

        var translationService = new PrefixMetadataTranslationService();
        using var appService = CreateAppService(translationService);
        RequestLanguage.CurrentLanguage = "en";

        try
        {
            var pageData = await appService.GetPagedBlogPostsAsync(pageIndex: 1, pageSize: 1);
            var post = Assert.Single(pageData.Data);

            Assert.Equal("[en]展示文章", post.Title);
            Assert.Equal(2, pageData.Total);
            var request = Assert.Single(translationService.Requests);
            Assert.Equal(ContentTranslationKind.JsonResource, request.Kind);
            Assert.Contains("展示文章", request.Source);
            Assert.DoesNotContain("未展示文章", request.Source);

            var localizedPostDir = Path.Combine(_tempRoot, "i18n", "en", "2026", "05");
            Assert.Single(Directory.GetFiles(localizedPostDir, "displayed-post.*.yml"));
            Assert.Empty(Directory.GetFiles(localizedPostDir, "hidden-post.*.yml"));
            Assert.Empty(Directory.GetFiles(localizedPostDir, "*.md"));
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
            I18nResourcesDir = Path.Combine(_tempRoot, "i18n"),
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
            I18nResourcesDir = Path.Combine(_tempRoot, "i18n"),
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

    private static Dictionary<string, string> ParseForm(string form)
    {
        return form
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(static item => item.Split('=', 2))
            .Where(static parts => parts.Length == 2)
            .ToDictionary(
                static parts => Uri.UnescapeDataString(parts[0].Replace("+", " ", StringComparison.Ordinal)),
                static parts => Uri.UnescapeDataString(parts[1].Replace("+", " ", StringComparison.Ordinal)),
                StringComparer.Ordinal);
    }

    private static string CreateMd5(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string TranslateStructuredBatchPayload(string source)
    {
        return Regex.Replace(
            source,
            @"(?m)^(?<id>\d{6})\s+(?<value>.*)$",
            static match => $"{match.Groups["id"].Value}\t[en]{match.Groups["value"].Value}");
    }

    private sealed class SingleClientHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public SingleClientHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;

        public CapturingHttpMessageHandler(string response)
        {
            _response = response;
        }

        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_response, Encoding.UTF8, "application/json")
            };
        }
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

    private sealed class ThrowingContentTranslationService : IContentTranslationService
    {
        public Task<string?> TranslateAsync(
            string source,
            LanguageInfo targetLanguage,
            ContentTranslationKind kind,
            string? resourceName = null,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Translation should not be called for existing localized content.");
    }

    private sealed class EmptyContentTranslationService : IContentTranslationService
    {
        private int _callCount;

        public int CallCount => _callCount;

        public Task<string?> TranslateAsync(
            string source,
            LanguageInfo targetLanguage,
            ContentTranslationKind kind,
            string? resourceName = null,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult<string?>(null);
        }
    }

    private sealed class ArticleSidecarTranslationService : IContentTranslationService
    {
        public Task<string?> TranslateAsync(
            string source,
            LanguageInfo targetLanguage,
            ContentTranslationKind kind,
            string? resourceName = null,
            CancellationToken cancellationToken = default)
        {
            if (kind == ContentTranslationKind.MarkdownPage)
            {
                return Task.FromResult<string?>("English body.");
            }

            return Task.FromResult<string?>("""
                {
                  "Title": "English title",
                  "Description": "English description",
                  "Albums": null,
                  "Categories": ["Web Development"],
                  "Tags": ["Website Rebuild"]
                }
                """);
        }
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

    private sealed class PrefixMetadataTranslationService : IContentTranslationService
    {
        public List<(ContentTranslationKind Kind, string Source)> Requests { get; } = [];

        public Task<string?> TranslateAsync(
            string source,
            LanguageInfo targetLanguage,
            ContentTranslationKind kind,
            string? resourceName = null,
            CancellationToken cancellationToken = default)
        {
            Requests.Add((kind, source));
            using var document = JsonDocument.Parse(source);
            var root = document.RootElement;
            var payload = new
            {
                Title = Prefix(root, "Title", targetLanguage.Code),
                Description = Prefix(root, "Description", targetLanguage.Code),
                Albums = PrefixArray(root, "Albums", targetLanguage.Code),
                Categories = PrefixArray(root, "Categories", targetLanguage.Code),
                Tags = PrefixArray(root, "Tags", targetLanguage.Code)
            };
            return Task.FromResult<string?>(JsonSerializer.Serialize(payload));
        }

        private static string? Prefix(JsonElement root, string propertyName, string language)
        {
            return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
                ? $"[{language}]{value.GetString()}"
                : null;
        }

        private static List<string>? PrefixArray(JsonElement root, string propertyName, string language)
        {
            return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
                ? value.EnumerateArray()
                    .Where(static item => item.ValueKind == JsonValueKind.String)
                    .Select(item => $"[{language}]{item.GetString()}")
                    .ToList()
                : null;
        }
    }
}
