using CodeWF.Web.Models;
using CodeWF.Web.Options;
using CodeWF.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeWF.Web.Tests;

public sealed class BlogPostFileServiceTests
{
    [Fact]
    public void Read_ParsesSidecarMetadataAndMarkdown()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "05");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "hello-codewf.md");
        File.WriteAllText(markdown, "# Hello\n\nContent");
        File.WriteAllText(BlogPostFileService.GetMetadataPath(markdown), """
title: "Hello CodeWF"
slug: "hello-codewf"
description: "A test post"
date: 2026-05-12 10:00:00
categories:
  - ".NET"
tags:
  - "CodeWF"
""");

        var service = new BlogPostFileService();
        var post = service.Read(markdown, root, "https://img1.dotnet9.com");

        Assert.Equal("Hello CodeWF", post.Title);
        Assert.Equal("hello-codewf", post.Slug);
        Assert.Equal(2026, post.Year);
        Assert.Equal(5, post.Month);
        Assert.Contains("<h1", post.HtmlContent);
    }

    [Fact]
    public async Task SearchAsync_BlocksConfiguredKeywordGroup()
    {
        var root = CreateTempRoot();
        WriteSearchAssets(root);
        var repository = CreateRepository(root);

        var result = await repository.SearchAsync("zh-CN", "赌博博彩", 1, 10);

        Assert.True(result.IsBlocked);
        Assert.Equal(0, result.Total);
        Assert.Contains("赌博博彩", result.Notice);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task SearchSuggestions_ReturnSnippetForMatchingPost()
    {
        var root = CreateTempRoot();
        WriteSearchAssets(root);
        var repository = CreateRepository(root);

        var suggestions = await repository.GetSearchSuggestionsAsync("zh-CN", "hello", 5);

        Assert.NotEmpty(suggestions);
        Assert.Equal("Hello CodeWF", suggestions[0].Title);
        Assert.Contains("hello", suggestions[0].MatchedSnippet ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetHomeAsync_UsesLastmodAndLimitsFeaturedContent()
    {
        var root = CreateTempRoot();
        WriteHomePosts(root);
        var repository = CreateRepository(root);

        var home = await repository.GetHomeAsync("zh-CN", 3);

        Assert.Equal(3, home.RecentPosts.Count);
        Assert.Equal("updated-by-lastmod", home.RecentPosts[0].Slug);
        Assert.Equal("banner-1", home.BannerPosts[0].Slug);
        Assert.Equal(3, home.BannerPosts.Count);
        Assert.DoesNotContain(home.BannerPosts, post => post.Slug == "banner-4");
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "codewf-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void WriteSearchAssets(string root)
    {
        var yearMonth = Path.Combine(root, "2026", "05");
        Directory.CreateDirectory(yearMonth);

        var markdown = Path.Combine(yearMonth, "hello-codewf.md");
        File.WriteAllText(markdown, """
---
title: "Hello CodeWF"
slug: "hello-codewf"
description: "A searchable post"
date: 2026-05-12 10:00:00
categories:
  - ".NET"
tags:
  - "CodeWF"
---

Hello world from CodeWF content.
""");

        File.WriteAllText(BlogPostFileService.GetMetadataPath(markdown), """
title: "Hello CodeWF"
slug: "hello-codewf"
description: "A searchable post"
date: 2026-05-12 10:00:00
categories:
  - ".NET"
tags:
  - "CodeWF"
""");

        var siteDir = Path.Combine(root, "site");
        Directory.CreateDirectory(siteDir);
        File.WriteAllText(Path.Combine(siteDir, "blocked-search-keywords.json"), """
[
  {
    "Sort": 1,
    "Name": "赌博博彩",
    "Memo": "过滤赌博、博彩、投注平台等不适宜搜索词",
    "Keywords": ["赌博", "博彩", "gambling"]
  }
]
""");
    }

    private static void WriteHomePosts(string root)
    {
        WritePost(root, "2026", "01", "updated-by-lastmod", """
title: "Updated by lastmod"
slug: "updated-by-lastmod"
description: "Should be the first recent post"
date: 2026-01-01 10:00:00
lastmod: 2026-05-12 09:00:00
""");

        WritePost(root, "2026", "05", "banner-1", """
title: "Banner 1"
slug: "banner-1"
description: "Banner post 1"
date: 2026-05-11 10:00:00
lastmod: 2026-05-11 09:00:00
banner: true
""");

        WritePost(root, "2026", "05", "banner-2", """
title: "Banner 2"
slug: "banner-2"
description: "Banner post 2"
date: 2026-05-10 10:00:00
lastmod: 2026-05-10 09:00:00
banner: true
""");

        WritePost(root, "2026", "05", "banner-3", """
title: "Banner 3"
slug: "banner-3"
description: "Banner post 3"
date: 2026-05-09 10:00:00
lastmod: 2026-05-09 09:00:00
banner: true
""");

        WritePost(root, "2026", "05", "banner-4", """
title: "Banner 4"
slug: "banner-4"
description: "Banner post 4"
date: 2026-05-08 10:00:00
lastmod: 2026-05-08 09:00:00
banner: true
""");
    }

    private static void WritePost(string root, string year, string month, string slug, string metadata)
    {
        var directory = Path.Combine(root, year, month);
        Directory.CreateDirectory(directory);

        var markdown = Path.Combine(directory, $"{slug}.md");
        File.WriteAllText(markdown, $"# {slug}\n\nBody");
        File.WriteAllText(BlogPostFileService.GetMetadataPath(markdown), metadata);
    }

    private static ContentRepository CreateRepository(string root)
    {
        var options = new SiteOptions
        {
            LocalAssetsDir = root,
            AssetBaseUrl = "https://img1.dotnet9.com"
        };

        return new ContentRepository(new TestOptionsMonitor<SiteOptions>(options), new MemoryCache(new MemoryCacheOptions()), new BlogPostFileService());
    }

    [Fact]
    public void Read_ParsesInlineFrontMatter_IncludingBanner()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "01");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "banner-post.md");
        File.WriteAllText(markdown, """
---
title: "Banner Post"
slug: "banner-post"
date: 2026-01-15 10:00:00
banner: true
cover: https://img1.dotnet9.com/2026/01/cover.png
---

正文
""");

        var service = new BlogPostFileService();
        var post = service.Read(markdown, root, "https://img1.dotnet9.com");

        Assert.True(post.Banner, "banner: true 应被反序列化");
        Assert.Equal("Banner Post", post.Title);
        Assert.False(post.Draft);
        Assert.Contains("正文", post.Content);
    }

    [Fact]
    public void Read_ParsesInlineFrontMatter_OutdatedFields()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "09");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "outdated-post.md");
        File.WriteAllText(markdown, """
---
title: "Outdated Post"
slug: "outdated-post"
date: 2026-09-23 19:59:14
outdated: true
outdatedMessage: "结论已被后续文章更新"
outdatedLink: /2026/11/newer-post
outdatedLinkText: "查看最新解读"
---

正文
""");

        var service = new BlogPostFileService();
        var post = service.Read(markdown, root, "https://img1.dotnet9.com");

        Assert.True(post.Outdated, "outdated: true 应被反序列化");
        Assert.Equal("结论已被后续文章更新", post.OutdatedMessage);
        Assert.Equal("/2026/11/newer-post", post.OutdatedLink);
        Assert.Equal("查看最新解读", post.OutdatedLinkText);
        Assert.Contains("正文", post.Content);
    }

    [Fact]
    public void Read_NormalizesCoverUrl_AbsoluteFormsStayUntouched()
    {
        var root = CreateTempRoot();

        var relative = WritePostWithCover(root, "rel-cover", "cover: 2026/01/rel-cover.png");
        var absolute = WritePostWithCover(root, "abs-cover", "cover: https://img1.dotnet9.com/2026/01/abs-cover.png");
        var rooted = WritePostWithCover(root, "rooted-cover", "cover: /images/rooted-cover.png");

        var service = new BlogPostFileService();

        // 资产库相对路径 → 拼 CDN 基址（域名可换，文章零改动）
        Assert.Equal("https://img1.dotnet9.com/2026/01/rel-cover.png",
            service.Read(relative, root, "https://img1.dotnet9.com").Cover);
        // 已是绝对 URL → 原样保留
        Assert.Equal("https://img1.dotnet9.com/2026/01/abs-cover.png",
            service.Read(absolute, root, "https://img1.dotnet9.com").Cover);
        // 站点根相对（/ 开头）→ 语义与正文 URL 一致，原样保留
        Assert.Equal("/images/rooted-cover.png",
            service.Read(rooted, root, "https://img1.dotnet9.com").Cover);
    }

    private static string WritePostWithCover(string root, string slug, string coverLine)
    {
        var directory = Path.Combine(root, "2026", "01");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, $"{slug}.md");
        File.WriteAllText(markdown, $"---\ntitle: \"{slug}\"\nslug: \"{slug}\"\ndate: 2026-01-10 10:00:00\n{coverLine}\n---\n\n正文\n");
        return markdown;
    }

    [Fact]
    public async Task Diag_RealRepo_BannerPosts()
    {
        var root = @"D:\github\Apps\Assets.Dotnet9";
        if (!Directory.Exists(root)) return; // 本机仓库存在时才诊断

        var repository = CreateRepository(root);
        var briefs = await repository.GetPostBriefsAsync("zh-CN");
        var home = await repository.GetHomeAsync("zh-CN", 6);

        Assert.True(briefs.Count > 0, $"briefs={briefs.Count}");
        Assert.True(home.BannerPosts.Count > 0, $"bannerPosts={home.BannerPosts.Count}, bannerTrueInBriefs={briefs.Count(b => b.Banner)}, total={briefs.Count}");
    }

    [Fact]
    public void Read_ClosingDelimiterWithTrailingWhitespace_StillSplitsFrontMatter()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "03");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "sloppy-close.md");
        // 闭合分隔行带尾随空格：旧实现字节精确匹配 "\n---\n" 会整体漏切，
        // YAML 渲染成 hr + setext 标题并泄进搜索索引
        File.WriteAllText(markdown, "---\ntitle: \"Sloppy Close\"\nslug: sloppy-close\ndate: 2026-03-01 10:00:00\n--- \n\n正文\n");

        var service = new BlogPostFileService();
        var post = service.Read(markdown, root, "https://img1.dotnet9.com");

        Assert.Equal("Sloppy Close", post.Title);
        Assert.Contains("正文", post.Content);
        Assert.DoesNotContain("title:", post.Content);
    }

    [Fact]
    public void Read_OpeningDelimiterWithTrailingWhitespace_StillSplitsFrontMatter()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "03");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "sloppy-open.md");
        File.WriteAllText(markdown, "--- \ntitle: \"Sloppy Open\"\nslug: sloppy-open\ndate: 2026-03-02 10:00:00\n---\n\n正文\n");

        var service = new BlogPostFileService();
        var post = service.Read(markdown, root, "https://img1.dotnet9.com");

        Assert.Equal("Sloppy Open", post.Title);
        Assert.Contains("正文", post.Content);
    }

    [Fact]
    public void Read_DocStartingWithRule_IsNotSwallowedAsFrontMatter()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "03");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "rule-opener.md");
        // 首行 --- 但并非 front matter（无 key: 行）：旧实现会把两根分隔线
        // 之间的正文整块吞进元数据再静默丢弃
        File.WriteAllText(markdown, "---\n这一段是正文散文。\n---\n\n## 真小节\n\n后续内容。\n");

        var service = new BlogPostFileService();
        var post = service.Read(markdown, root, "https://img1.dotnet9.com");

        Assert.Contains("这一段是正文散文。", post.Content);
        Assert.Contains("后续内容。", post.Content);
        Assert.Contains("这一段是正文散文。", post.HtmlContent);
    }

    [Fact]
    public void Read_UnclosedOpeningRule_KeepsWholeFileAsContent()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "03");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "unclosed.md");
        File.WriteAllText(markdown, "---\ntitle: 未闭合\n\n正文段落。\n");

        var service = new BlogPostFileService();
        var post = service.Read(markdown, root, "https://img1.dotnet9.com");

        // 无闭合行不认 front matter，整篇保留为正文（slug 走文件名兜底）
        Assert.Contains("正文段落。", post.Content);
        Assert.Equal("unclosed", post.Slug);
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private readonly T value;

        public TestOptionsMonitor(T value)
        {
            this.value = value;
        }

        public T CurrentValue => value;

        public T Get(string? name) => value;

        public IDisposable OnChange(Action<T, string?> listener) => new NoopDisposable();
    }

    private sealed class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
