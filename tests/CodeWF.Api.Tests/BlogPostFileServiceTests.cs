using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeWF.Api.Tests;

public sealed class BlogPostFileServiceTests
{
    [Fact]
    public async Task ReadAsync_ParsesSidecarMetadataAndMarkdown()
    {
        var root = CreateTempRoot();
        var directory = Path.Combine(root, "2026", "05");
        Directory.CreateDirectory(directory);
        var markdown = Path.Combine(directory, "hello-codewf.md");
        await File.WriteAllTextAsync(markdown, "# Hello\n\nContent");
        await File.WriteAllTextAsync(BlogPostFileService.GetMetadataPath(markdown), """
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
        var post = await service.ReadAsync(markdown, root, "https://img1.dotnet9.com");

        Assert.Equal("Hello CodeWF", post.Title);
        Assert.Equal("hello-codewf", post.Slug);
        Assert.Equal(2026, post.Year);
        Assert.Equal(5, post.Month);
        Assert.Contains("<h1", post.HtmlContent);
    }

    [Fact]
    public async Task WriteAsync_WritesMarkdownAndMetadata()
    {
        var root = CreateTempRoot();
        var markdown = Path.Combine(root, "2026", "05", "new-post.md");
        var service = new BlogPostFileService();

        await service.WriteAsync(markdown, new AdminPostRequest
        {
            Title = "New Post",
            Slug = "new-post",
            Description = "Desc",
            Date = new DateTime(2026, 5, 12),
            Content = "Body",
            Categories = ["ASP.NET Core"],
            Tags = ["API"]
        });

        Assert.True(File.Exists(markdown));
        Assert.True(File.Exists(BlogPostFileService.GetMetadataPath(markdown)));
        Assert.Contains("title: \"New Post\"", await File.ReadAllTextAsync(BlogPostFileService.GetMetadataPath(markdown)));
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
