using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodeWF.Api.Models;
using CodeWF.Api.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Services;

public sealed partial class ContentRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly IOptionsMonitor<SiteOptions> siteOptions;
    private readonly IMemoryCache cache;
    private readonly BlogPostFileService postFiles;
    private int cacheVersion;

    public ContentRepository(
        IOptionsMonitor<SiteOptions> siteOptions,
        IMemoryCache cache,
        BlogPostFileService postFiles)
    {
        this.siteOptions = siteOptions;
        this.cache = cache;
        this.postFiles = postFiles;
    }

    public SiteInfo GetSiteInfo()
    {
        var options = siteOptions.CurrentValue;
        return new SiteInfo(
            options.AppTitle,
            options.Domain,
            options.Memo,
            options.Owner,
            options.OwnerDesc,
            options.Favicon,
            options.LocalAssetsDir,
            options.AssetBaseUrl.TrimEnd('/'),
            options.RemoteAssetsRepository,
            options.StartYear,
            NormalizeCulture(options.DefaultCulture),
            options.SupportedCultures.Select(NormalizeCulture).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            options.BaiAn,
            options.WeChatName,
            options.WeChatImg);
    }

    public async Task<HomePageData> GetHomeAsync(string culture, int recent)
    {
        var posts = (await GetPostBriefsAsync(culture)).ToList();
        var orderedPosts = posts
            .OrderByDescending(static post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ToList();
        var bannerPosts = orderedPosts
            .Where(static post => post.Banner)
            .Take(3)
            .ToList();
        var tools = await GetToolsAsync(culture);
        var categories = await GetCategoriesAsync(culture);
        var albums = await GetAlbumsAsync(culture);
        return new HomePageData(
            GetSiteInfo(),
            orderedPosts.Take(Math.Clamp(recent, 1, 24)).ToList(),
            bannerPosts,
            categories,
            albums,
            tools,
            new Dictionary<string, int>
            {
                ["posts"] = posts.Count,
                ["tools"] = FlattenTools(tools).Count(),
                ["docs"] = FlattenDocs(await GetDocsAsync(culture)).Count(),
                ["categories"] = categories.Count,
                ["albums"] = albums.Count
            });
    }

    public async Task<IReadOnlyDictionary<string, string>> GetLocalizationAsync(string culture)
    {
        var localized = await ReadJsonAsync<Dictionary<string, string>>(culture, "site", "lang.json");
        return localized ?? new Dictionary<string, string>();
    }

    public Task<IReadOnlyList<ToolNode>> GetToolsAsync(string culture) =>
        GetCachedAsync($"tools:{NormalizeCulture(culture)}", async () =>
            (IReadOnlyList<ToolNode>)(await ReadJsonAsync<List<ToolNode>>(culture, "site", "tools", "tools.json") ?? []));

    public async Task<ToolNode?> GetToolAsync(string culture, string slug)
    {
        var tools = await GetToolsAsync(culture);
        return FlattenTools(tools).FirstOrDefault(tool => SlugEquals(tool.Slug, slug));
    }

    public Task<IReadOnlyList<DocNode>> GetDocsAsync(string culture) =>
        GetCachedAsync($"docs:{NormalizeCulture(culture)}", async () =>
            (IReadOnlyList<DocNode>)(await ReadJsonAsync<List<DocNode>>(culture, "site", "doc", "navigation.json") ?? []));

    public async Task<DocNode?> GetDocAsync(string culture, string slug)
    {
        var docs = await GetDocsAsync(culture);
        var found = FindDocNode(docs, slug, []);
        if (found is null)
        {
            return null;
        }

        var (node, pathSegments) = found.Value;
        var flatDocs = FlattenDocs(docs).ToList();
        var currentIndex = flatDocs.FindIndex(item => SlugEquals(item.Slug, node.Slug));
        var markdownPath = ResolveLocalizedPath(culture, Path.Combine(["site", "doc", .. pathSegments, $"{node.Slug}.md"]));
        if (markdownPath is null || !File.Exists(markdownPath))
        {
            return CloneDocDetail(node, flatDocs, currentIndex);
        }

        var markdown = await File.ReadAllTextAsync(markdownPath, Encoding.UTF8);
        return new DocNode
        {
            Name = node.Name,
            Memo = node.Memo,
            Slug = node.Slug,
            Repository = node.Repository,
            Content = markdown,
            HtmlContent = postFiles.RenderMarkdown(markdown, markdownPath, AssetsRoot(), siteOptions.CurrentValue.AssetBaseUrl),
            PreviousDoc = currentIndex > 0 ? ToDocSummary(flatDocs[currentIndex - 1]) : null,
            NextDoc = currentIndex >= 0 && currentIndex < flatDocs.Count - 1 ? ToDocSummary(flatDocs[currentIndex + 1]) : null
        };
    }

    public Task<IReadOnlyList<TaxonomyItem>> GetCategoriesAsync(string culture) =>
        GetTaxonomyAsync(culture, "categories.json", "Categories");

    public Task<IReadOnlyList<TaxonomyItem>> GetAlbumsAsync(string culture) =>
        GetTaxonomyAsync(culture, "albums.json", "Albums");

    public async Task<IReadOnlyList<TagItem>> GetTagsAsync(string culture)
    {
        var posts = await GetPostBriefsAsync(culture);
        return posts.SelectMany(static post => post.Tags ?? [])
            .Where(static tag => !string.IsNullOrWhiteSpace(tag))
            .GroupBy(static tag => tag.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(static group => new TagItem { Name = group.Key, PostCount = group.Count() })
            .OrderByDescending(static tag => tag.PostCount)
            .ThenBy(static tag => tag.Name)
            .ToList();
    }

    public Task<IReadOnlyList<SearchBlockedKeywordGroup>> GetBlockedSearchKeywordsAsync(string culture) =>
        GetCachedAsync($"blocked-search-keywords:{NormalizeCulture(culture)}", async () =>
            (IReadOnlyList<SearchBlockedKeywordGroup>)(await ReadJsonAsync<List<SearchBlockedKeywordGroup>>(culture, "site", "blocked-search-keywords.json") ?? []));

    public Task<IReadOnlyList<FriendLinkItem>> GetFriendLinksAsync(string culture) =>
        GetCachedAsync($"friend-links:{NormalizeCulture(culture)}", async () =>
            (IReadOnlyList<FriendLinkItem>)(await ReadJsonAsync<List<FriendLinkItem>>(culture, "site", "friend-links.json") ?? []));

    public Task<IReadOnlyList<TimelineItem>> GetTimelinesAsync(string culture) =>
        GetCachedAsync($"timelines:{NormalizeCulture(culture)}", async () =>
            (IReadOnlyList<TimelineItem>)(await ReadJsonAsync<List<TimelineItem>>(culture, "site", "timelines.json") ?? []));

    public async Task<MarkdownPage> ReadSitePageAsync(string culture, params string[] relativeSegments)
    {
        var path = ResolveLocalizedPath(culture, Path.Combine(["site", .. relativeSegments]));
        if (path is null || !File.Exists(path))
        {
            return new MarkdownPage(null, null);
        }

        var markdown = await File.ReadAllTextAsync(path, Encoding.UTF8);
        return new MarkdownPage(markdown, postFiles.RenderMarkdown(markdown, path, AssetsRoot(), siteOptions.CurrentValue.AssetBaseUrl));
    }

    public async Task<EditableMarkdownResource> ReadMarkdownResourceAsync(string culture, string name, params string[] relativeSegments)
    {
        var path = ResolveLocalizedPath(culture, Path.Combine(["site", .. relativeSegments]));
        if (path is null || !File.Exists(path))
        {
            return new EditableMarkdownResource(name, string.Empty, null, null);
        }

        var markdown = await File.ReadAllTextAsync(path, Encoding.UTF8);
        return new EditableMarkdownResource(
            name,
            path,
            markdown,
            postFiles.RenderMarkdown(markdown, path, AssetsRoot(), siteOptions.CurrentValue.AssetBaseUrl));
    }

    public async Task<EditableJsonResource> ReadJsonResourceAsync(string culture, string name, params string[] relativeSegments)
    {
        var path = ResolveLocalizedPath(culture, Path.Combine(["site", .. relativeSegments]));
        if (path is null || !File.Exists(path))
        {
            return new EditableJsonResource(name, string.Empty, null);
        }

        var json = await File.ReadAllTextAsync(path, Encoding.UTF8);
        return new EditableJsonResource(name, path, json);
    }

    public async Task<AdminMutationResult> SaveMarkdownResourceAsync(string culture, string name, string content, params string[] relativeSegments)
    {
        var path = GetWritableLocalizedPath(culture, Path.Combine(["site", .. relativeSegments]));
        if (path is null)
        {
            return new AdminMutationResult(false, $"Invalid markdown resource path: {name}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, NormalizeTextContent(content), Encoding.UTF8);
        Invalidate();
        return new AdminMutationResult(true, $"Saved {name}.");
    }

    public async Task<AdminMutationResult> SaveJsonResourceAsync(string culture, string name, string content, params string[] relativeSegments)
    {
        var path = GetWritableLocalizedPath(culture, Path.Combine(["site", .. relativeSegments]));
        if (path is null)
        {
            return new AdminMutationResult(false, $"Invalid JSON resource path: {name}");
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var formatted = JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, formatted + Environment.NewLine, Encoding.UTF8);
            Invalidate();
            return new AdminMutationResult(true, $"Saved {name}.");
        }
        catch (JsonException ex)
        {
            return new AdminMutationResult(false, $"Invalid JSON for {name}: {ex.Message}");
        }
    }

    public Task<AssetDirectoryData> GetAssetsAsync(string relativePath = "") =>
        GetCachedAsync($"assets:{relativePath}", async () =>
        {
            var root = AssetsRoot();
            if (!Directory.Exists(root))
            {
                return new AssetDirectoryData(root, string.Empty, []);
            }

            var fullPath = ResolveAssetDirectoryPath(relativePath);
            if (fullPath is null || !Directory.Exists(fullPath))
            {
                return new AssetDirectoryData(root, string.Empty, []);
            }

            var entries = Directory.EnumerateFileSystemEntries(fullPath)
                .Select(path =>
                {
                    var info = File.GetAttributes(path).HasFlag(FileAttributes.Directory)
                        ? (FileSystemInfo)new DirectoryInfo(path)
                        : new FileInfo(path);

                    var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
                    return new AssetEntry(
                        info.Name,
                        relative,
                        (info.Attributes & FileAttributes.Directory) != 0,
                        info is FileInfo fileInfo ? fileInfo.Length : null,
                        info.LastWriteTimeUtc);
                })
                .OrderByDescending(entry => entry.IsDirectory)
                .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new AssetDirectoryData(root, GetRelativeAssetPath(fullPath), entries);
        });

    public async Task<AssetUploadResult> SaveAssetAsync(string relativePath, string fileName, Stream content)
    {
        var directory = ResolveAssetDirectoryPath(relativePath);
        if (directory is null)
        {
            return new AssetUploadResult(false, "Invalid asset path.");
        }

        Directory.CreateDirectory(directory);
        var target = Path.GetFullPath(Path.Combine(directory, Path.GetFileName(fileName)));
        var root = AssetsRoot();
        if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return new AssetUploadResult(false, "Invalid asset target path.");
        }

        await using var fileStream = File.Create(target);
        await content.CopyToAsync(fileStream);
        Invalidate();
        return new AssetUploadResult(true, "Uploaded.", GetRelativeAssetPath(target));
    }

    public Task<AssetUploadResult> DeleteAssetAsync(string relativePath)
    {
        var fullPath = ResolveAssetPath(relativePath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return Task.FromResult(new AssetUploadResult(false, "Asset not found."));
        }

        File.Delete(fullPath);
        Invalidate();
        return Task.FromResult(new AssetUploadResult(true, "Deleted.", GetRelativeAssetPath(fullPath)));
    }

    public async Task<IReadOnlyList<BlogPostBrief>> GetPostBriefsAsync(
        string culture,
        bool includeDrafts = false)
    {
        var posts = await GetAllPostsAsync(culture);
        return posts
            .Where(post => includeDrafts || !post.Draft)
            .Select(ToBrief)
            .OrderByDescending(static post => post.Date)
            .ThenByDescending(static post => post.Lastmod)
            .ToList();
    }

    public async Task<PagedResult<BlogPostBrief>> GetPagedPostsAsync(
        string culture,
        int pageIndex,
        int pageSize,
        string? keyword = null,
        string? category = null,
        string? album = null,
        string? tag = null,
        bool includeDrafts = false)
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = (await GetPostBriefsAsync(culture, includeDrafts)).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(post =>
                Contains(post.Title, keyword)
                || Contains(post.Description, keyword)
                || Contains(post.Slug, keyword)
                || ContainsAny(post.Categories, keyword)
                || ContainsAny(post.Albums, keyword)
                || ContainsAny(post.Tags, keyword));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(post => post.Categories?.Any(item => SlugEquals(item, category)) == true);
        }

        if (!string.IsNullOrWhiteSpace(album))
        {
            query = query.Where(post => post.Albums?.Any(item => SlugEquals(item, album)) == true);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(post => post.Tags?.Any(item => string.Equals(item, tag, StringComparison.OrdinalIgnoreCase)) == true);
        }

        var filtered = query.ToList();
        return new PagedResult<BlogPostBrief>(
            pageIndex,
            pageSize,
            filtered.Count,
            filtered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
    }

    public async Task<BlogPost?> GetPostAsync(
        string culture,
        string slug,
        int? year = null,
        int? month = null,
        bool includeDrafts = false)
    {
        var posts = await GetAllPostsAsync(culture);
        var post = posts.FirstOrDefault(post =>
            (includeDrafts || !post.Draft)
            && SlugEquals(post.Slug, slug)
            && (!year.HasValue || post.Year == year.Value)
            && (!month.HasValue || post.Month == month.Value));
        return post is null ? null : EnrichPostDetail(post, posts.Where(item => includeDrafts || !item.Draft));
    }

    public async Task<SearchResultPageData> SearchAsync(
        string culture,
        string? query,
        int pageIndex,
        int pageSize,
        SearchResultKind? kind = null)
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var keyword = query?.Trim();

        var posts = (await GetAllPostsAsync(culture)).Where(static post => !post.Draft).ToList();
        var tools = FlattenTools(await GetToolsAsync(culture)).ToList();
        var docs = FlattenDocs(await GetDocsAsync(culture)).ToList();
        var blockedGroup = FindBlockedSearchKeywordGroup(keyword, await GetBlockedSearchKeywordsAsync(culture));

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new SearchResultPageData(pageIndex, pageSize, 0, [], tools.Count, docs.Count, posts.Count);
        }

        if (blockedGroup is not null)
        {
            return new SearchResultPageData(
                pageIndex,
                pageSize,
                0,
                [],
                tools.Count,
                docs.Count,
                posts.Count,
                true,
                string.IsNullOrWhiteSpace(blockedGroup.Memo)
                    ? blockedGroup.Name
                    : $"{blockedGroup.Name}：{blockedGroup.Memo}");
        }

        var results = new List<SearchResultItem>();
        if (kind is null or SearchResultKind.Post)
        {
            results.AddRange(posts.Select(post => BuildSearchResult(post, keyword)).Where(static item => item.Score > 0));
        }

        if (kind is null or SearchResultKind.Tool)
        {
            results.AddRange(tools.Select(tool => BuildSearchResult(tool, keyword)).Where(static item => item.Score > 0));
        }

        if (kind is null or SearchResultKind.Doc)
        {
            results.AddRange(docs.Select(doc => BuildSearchResult(doc, keyword)).Where(static item => item.Score > 0));
        }

        var ordered = results.OrderByDescending(static item => item.Score).ThenByDescending(static item => item.UpdatedAt).ToList();
        return new SearchResultPageData(
            pageIndex,
            pageSize,
            ordered.Count,
            ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(),
            tools.Count,
            docs.Count,
            posts.Count);
    }

    public async Task<IReadOnlyList<SearchResultItem>> GetSearchSuggestionsAsync(string culture, string? query, int take = 10)
    {
        take = Math.Clamp(take, 1, 20);
        var keyword = query?.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        if (FindBlockedSearchKeywordGroup(keyword, await GetBlockedSearchKeywordsAsync(culture)) is not null)
        {
            return [];
        }

        var posts = (await GetAllPostsAsync(culture)).Where(static post => !post.Draft);
        var tools = FlattenTools(await GetToolsAsync(culture));
        var docs = FlattenDocs(await GetDocsAsync(culture));

        var results = new List<SearchResultItem>();
        results.AddRange(posts.Select(post => BuildSearchResult(post, keyword)).Where(static item => item.Score > 0));
        results.AddRange(tools.Select(tool => BuildSearchResult(tool, keyword)).Where(static item => item.Score > 0));
        results.AddRange(docs.Select(doc => BuildSearchResult(doc, keyword)).Where(static item => item.Score > 0));

        return results
            .OrderByDescending(static item => item.Score)
            .ThenByDescending(static item => item.UpdatedAt)
            .Take(take)
            .ToList();
    }

    public async Task<AdminMutationResult> UpsertPostAsync(AdminPostRequest request, string? currentSlug = null)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new AdminMutationResult(false, "Title is required.");
        }

        request.Slug = Slugify(string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);
        request.Date ??= DateTime.Today;
        request.Lastmod ??= DateTime.Now;

        var root = AssetsRoot();
        if (!Directory.Exists(root))
        {
            return new AdminMutationResult(false, $"Assets directory does not exist: {root}");
        }

        var targetPath = Path.Combine(root, request.Date.Value.Year.ToString("D4"), request.Date.Value.Month.ToString("D2"), $"{request.Slug}.md");
        string? previousMarkdownPath = null;
        if (!string.IsNullOrWhiteSpace(currentSlug))
        {
            previousMarkdownPath = (await GetPostAsync(SiteOptions.DefaultCultureName, currentSlug, includeDrafts: true))?.SourcePath;
        }

        await postFiles.WriteAsync(targetPath, request);

        if (!string.IsNullOrWhiteSpace(previousMarkdownPath)
            && !string.Equals(previousMarkdownPath, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            DeleteFileIfExists(previousMarkdownPath);
            DeleteFileIfExists(BlogPostFileService.GetMetadataPath(previousMarkdownPath));
        }

        Invalidate();
        var saved = await GetPostAsync(SiteOptions.DefaultCultureName, request.Slug, includeDrafts: true);
        return new AdminMutationResult(true, "Saved.", saved is null ? null : ToBrief(saved));
    }

    public async Task<bool> DeletePostAsync(string slug)
    {
        var post = await GetPostAsync(SiteOptions.DefaultCultureName, slug, includeDrafts: true);
        if (post?.SourcePath is null)
        {
            return false;
        }

        DeleteFileIfExists(post.SourcePath);
        DeleteFileIfExists(BlogPostFileService.GetMetadataPath(post.SourcePath));
        Invalidate();
        return true;
    }

    public async Task<string> GetRssAsync()
    {
        var posts = (await GetPostBriefsAsync(SiteOptions.DefaultCultureName)).Take(20).ToList();
        var site = GetSiteInfo();
        var domain = site.Domain.TrimEnd('/');
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<rss xmlns:atom=\"http://www.w3.org/2005/Atom\" version=\"2.0\">");
        sb.AppendLine("<channel>");
        sb.AppendLine($"<title>{Xml(site.AppTitle)}</title>");
        sb.AppendLine($"<link>{Xml(domain)}</link>");
        sb.AppendLine($"<description>{Xml(site.Memo)}</description>");
        sb.AppendLine("<language>zh-cn</language>");
        foreach (var post in posts)
        {
            var url = $"{domain}{post.Url}";
            sb.AppendLine("<item>");
            sb.AppendLine($"<title>{Xml(post.Title)}</title>");
            sb.AppendLine($"<link>{Xml(url)}</link>");
            sb.AppendLine($"<guid>{Xml(url)}</guid>");
            sb.AppendLine($"<description>{Xml(post.Description)}</description>");
            if (post.Date.HasValue)
            {
                sb.AppendLine($"<pubDate>{post.Date.Value:R}</pubDate>");
            }

            sb.AppendLine("</item>");
        }

        sb.AppendLine("</channel>");
        sb.AppendLine("</rss>");
        return sb.ToString();
    }

    public async Task<string> GetSitemapAsync()
    {
        var site = GetSiteInfo();
        var domain = site.Domain.TrimEnd('/');
        var urls = new SortedDictionary<string, DateTime?>(StringComparer.OrdinalIgnoreCase)
        {
            [$"{domain}/"] = DateTime.UtcNow,
            [$"{domain}/post"] = DateTime.UtcNow,
            [$"{domain}/tool"] = DateTime.UtcNow,
            [$"{domain}/project"] = DateTime.UtcNow,
            [$"{domain}/about"] = DateTime.UtcNow
        };

        foreach (var post in await GetPostBriefsAsync(SiteOptions.DefaultCultureName))
        {
            if (!string.IsNullOrWhiteSpace(post.Url))
            {
                urls[$"{domain}{post.Url}"] = post.Lastmod ?? post.Date;
            }
        }

        foreach (var tool in FlattenTools(await GetToolsAsync(SiteOptions.DefaultCultureName)).Where(static tool => !string.IsNullOrWhiteSpace(tool.Slug)))
        {
            urls[$"{domain}/tool/{tool.Slug}"] = DateTime.UtcNow;
        }

        foreach (var doc in FlattenDocs(await GetDocsAsync(SiteOptions.DefaultCultureName)).Where(static doc => !string.IsNullOrWhiteSpace(doc.Slug)))
        {
            urls[$"{domain}/project/{doc.Slug}"] = DateTime.UtcNow;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var (url, lastModified) in urls)
        {
            sb.AppendLine("<url>");
            sb.AppendLine($"<loc>{Xml(url)}</loc>");
            if (lastModified.HasValue)
            {
                sb.AppendLine($"<lastmod>{lastModified.Value:yyyy-MM-dd}</lastmod>");
            }

            sb.AppendLine("</url>");
        }

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }

    private Task<IReadOnlyList<TaxonomyItem>> GetTaxonomyAsync(string culture, string fileName, string propertyName) =>
        GetCachedAsync($"{fileName}:{NormalizeCulture(culture)}", async () =>
        {
            var items = await ReadJsonAsync<List<TaxonomyItem>>(culture, "site", fileName) ?? [];
            var posts = await GetPostBriefsAsync(culture);
            foreach (var item in items)
            {
                item.PostCount = posts.Count(post =>
                    string.Equals(propertyName, "Categories", StringComparison.OrdinalIgnoreCase)
                        ? post.Categories?.Any(value => SlugEquals(value, item.Slug) || string.Equals(value, item.Name, StringComparison.OrdinalIgnoreCase)) == true
                        : post.Albums?.Any(value => SlugEquals(value, item.Slug) || string.Equals(value, item.Name, StringComparison.OrdinalIgnoreCase)) == true);
            }

            return (IReadOnlyList<TaxonomyItem>)items.OrderBy(static item => item.Sort).ThenBy(static item => item.Name).ToList();
        });

    private Task<IReadOnlyList<BlogPost>> GetAllPostsAsync(string culture) =>
        GetCachedAsync($"posts:{NormalizeCulture(culture)}", async () =>
        {
            var root = AssetsRoot();
            if (!Directory.Exists(root))
            {
                return (IReadOnlyList<BlogPost>)[];
            }

            var posts = new List<BlogPost>();
            var yearDirectories = Directory.EnumerateDirectories(root)
                .Where(static dir => int.TryParse(Path.GetFileName(dir), out _))
                .OrderByDescending(static dir => dir)
                .ToList();

            foreach (var yearDirectory in yearDirectories)
            {
                foreach (var markdownPath in Directory.EnumerateFiles(yearDirectory, "*.md", SearchOption.AllDirectories))
                {
                    if (IsLocalizedSibling(markdownPath))
                    {
                        continue;
                    }

                    var localizedPath = ResolveLocalizedPath(culture, Path.GetRelativePath(root, markdownPath)) ?? markdownPath;
                    var metadataPath = ResolveLocalizedPath(culture, Path.GetRelativePath(root, BlogPostFileService.GetMetadataPath(markdownPath)))
                                       ?? BlogPostFileService.GetMetadataPath(markdownPath);

                    try
                    {
                        var post = await postFiles.ReadAsync(localizedPath, root, siteOptions.CurrentValue.AssetBaseUrl, metadataPath);
                        posts.Add(post);
                    }
                    catch
                    {
                        // Bad content files should not take the whole site down.
                    }
                }
            }

            return posts
                .OrderByDescending(static post => post.Date)
                .ThenByDescending(static post => post.Lastmod)
                .ToList();
        });

    private async Task<T?> ReadJsonAsync<T>(string culture, params string[] relativeSegments)
    {
        var path = ResolveLocalizedPath(culture, Path.Combine(relativeSegments));
        if (path is null || !File.Exists(path))
        {
            return default;
        }

        var content = await File.ReadAllTextAsync(path, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    private string? ResolveLocalizedPath(string culture, string relativePath)
    {
        var root = AssetsRoot();
        var defaultPath = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!defaultPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalized = NormalizeCulture(culture);
        var suffix = CultureSuffix(normalized);
        if (suffix is not null)
        {
            var directory = Path.GetDirectoryName(defaultPath)!;
            var extension = Path.GetExtension(defaultPath);
            var fileName = Path.GetFileNameWithoutExtension(defaultPath);
            var localized = Path.Combine(directory, $"{fileName}.{suffix}{extension}");
            if (File.Exists(localized))
            {
                return localized;
            }
        }

        return File.Exists(defaultPath) ? defaultPath : null;
    }

    private string? GetWritableLocalizedPath(string culture, string relativePath)
    {
        var root = AssetsRoot();
        var defaultPath = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!defaultPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalized = NormalizeCulture(culture);
        var suffix = CultureSuffix(normalized);
        if (suffix is null)
        {
            return defaultPath;
        }

        var directory = Path.GetDirectoryName(defaultPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        var extension = Path.GetExtension(defaultPath);
        var fileName = Path.GetFileNameWithoutExtension(defaultPath);
        return Path.Combine(directory, $"{fileName}.{suffix}{extension}");
    }

    private static string NormalizeTextContent(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private string AssetsRoot()
    {
        var configured = siteOptions.CurrentValue.LocalAssetsDir;
        var expanded = Environment.ExpandEnvironmentVariables(configured);
        return Path.GetFullPath(expanded);
    }

    private string? ResolveAssetPath(string relativePath)
    {
        var root = AssetsRoot();
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath ?? string.Empty));
        return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? fullPath : null;
    }

    private string? ResolveAssetDirectoryPath(string relativePath)
    {
        var root = AssetsRoot();
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath ?? string.Empty));
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return fullPath;
    }

    private string GetRelativeAssetPath(string fullPath) =>
        Path.GetRelativePath(AssetsRoot(), fullPath).Replace('\\', '/');

    private Task<T> GetCachedAsync<T>(string key, Func<Task<T>> factory) =>
        cache.GetOrCreateAsync($"{cacheVersion}:{key}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return factory();
        })!;

    private void Invalidate() => Interlocked.Increment(ref cacheVersion);

    private static BlogPost EnrichPostDetail(BlogPost source, IEnumerable<BlogPost> allPosts)
    {
        var orderedPosts = allPosts
            .OrderByDescending(static post => post.Date)
            .ThenByDescending(static post => post.Lastmod)
            .ToList();
        var currentIndex = orderedPosts.FindIndex(post => SlugEquals(post.Slug, source.Slug));
        var detail = ClonePost(source);

        detail.PreviousPost = currentIndex > 0 ? ToBrief(orderedPosts[currentIndex - 1]) : null;
        detail.NextPost = currentIndex >= 0 && currentIndex < orderedPosts.Count - 1 ? ToBrief(orderedPosts[currentIndex + 1]) : null;
        detail.RelatedPosts = SelectRelatedPosts(source, orderedPosts, 4);
        detail.EstimatedReadingMinutes = EstimateReadingMinutes(source.HtmlContent ?? source.Content);
        detail.HeadingCount = CountArticleHeadings(source.HtmlContent ?? source.Content);
        return detail;
    }

    private static BlogPost ClonePost(BlogPost post) =>
        new()
        {
            SourcePath = post.SourcePath,
            Title = post.Title,
            Slug = post.Slug,
            Description = post.Description,
            Date = post.Date,
            Lastmod = post.Lastmod,
            Copyright = post.Copyright,
            Banner = post.Banner,
            Author = post.Author,
            LastModifyUser = post.LastModifyUser,
            OriginalTitle = post.OriginalTitle,
            OriginalLink = post.OriginalLink,
            Draft = post.Draft,
            Cover = post.Cover,
            Albums = post.Albums,
            Categories = post.Categories,
            Tags = post.Tags,
            Url = post.Url,
            Year = post.Year,
            Month = post.Month,
            Content = post.Content,
            HtmlContent = post.HtmlContent
        };

    private static BlogPostBrief ToBrief(BlogPost post) =>
        new()
        {
            SourcePath = post.SourcePath,
            Title = post.Title,
            Slug = post.Slug,
            Description = post.Description,
            Date = post.Date,
            Lastmod = post.Lastmod,
            Copyright = post.Copyright,
            Banner = post.Banner,
            Author = post.Author,
            LastModifyUser = post.LastModifyUser,
            OriginalTitle = post.OriginalTitle,
            OriginalLink = post.OriginalLink,
            Draft = post.Draft,
            Cover = post.Cover,
            Albums = post.Albums,
            Categories = post.Categories,
            Tags = post.Tags,
            Url = post.Url,
            Year = post.Year,
            Month = post.Month,
            ContextLabel = post.ContextLabel
        };

    private static List<BlogPostBrief> SelectRelatedPosts(BlogPost currentPost, IReadOnlyList<BlogPost> posts, int limit)
    {
        var currentCategories = ToSet(currentPost.Categories);
        var currentAlbums = ToSet(currentPost.Albums);
        var currentTags = ToSet(currentPost.Tags);
        var scored = posts
            .Where(post => !SlugEquals(post.Slug, currentPost.Slug) && !string.IsNullOrWhiteSpace(post.Title))
            .Select(post =>
            {
                var score = 0;
                var reasons = new List<string>();

                var categoryScore = CountOverlap(currentCategories, post.Categories);
                if (categoryScore > 0)
                {
                    score += categoryScore * 6;
                    reasons.Add("同分类");
                }

                var albumScore = CountOverlap(currentAlbums, post.Albums);
                if (albumScore > 0)
                {
                    score += albumScore * 5;
                    reasons.Add("同专题");
                }

                var tagScore = CountOverlap(currentTags, post.Tags);
                if (tagScore > 0)
                {
                    score += tagScore * 4;
                    reasons.Add("同标签");
                }

                return new { Post = post, Score = score, ContextLabel = string.Join(" / ", reasons.Take(2)) };
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Post.Lastmod ?? item.Post.Date ?? DateTime.MinValue)
            .Take(limit)
            .Select(item =>
            {
                var brief = ToBrief(item.Post);
                brief.ContextLabel = item.ContextLabel;
                return brief;
            })
            .ToList();

        if (scored.Count >= limit)
        {
            return scored;
        }

        var existing = scored.Select(static post => post.Url ?? post.Slug ?? string.Empty).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var post in posts
                     .Where(post => !SlugEquals(post.Slug, currentPost.Slug) && !string.IsNullOrWhiteSpace(post.Title))
                     .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue))
        {
            var brief = ToBrief(post);
            var key = brief.Url ?? brief.Slug ?? string.Empty;
            if (!existing.Add(key))
            {
                continue;
            }

            brief.ContextLabel = "近期更新";
            scored.Add(brief);
            if (scored.Count >= limit)
            {
                break;
            }
        }

        return scored;
    }

    private static HashSet<string> ToSet(IEnumerable<string>? values) =>
        values?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? [];

    private static int CountOverlap(HashSet<string> currentValues, IEnumerable<string>? candidateValues)
    {
        if (currentValues.Count == 0 || candidateValues is null)
        {
            return 0;
        }

        return candidateValues.Count(value => !string.IsNullOrWhiteSpace(value) && currentValues.Contains(value.Trim()));
    }

    private static int CountArticleHeadings(string? content) =>
        string.IsNullOrWhiteSpace(content)
            ? 0
            : Regex.Matches(content, "<h[2-4]\\b", RegexOptions.IgnoreCase).Count;

    private static int EstimateReadingMinutes(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return 1;
        }

        var plainText = Regex.Replace(content, "<[^>]+>", " ");
        plainText = WebUtility.HtmlDecode(plainText);
        var cjkCharacters = Regex.Matches(plainText, "[\\u4e00-\\u9fff]").Count;
        var latinWords = Regex.Matches(plainText, "[A-Za-z0-9_]+").Count;
        return Math.Max(1, (int)Math.Ceiling((cjkCharacters + latinWords * 2) / 320d));
    }

    private static DocNode CloneDocDetail(DocNode node, IReadOnlyList<DocNode> flatDocs, int currentIndex) =>
        new()
        {
            Name = node.Name,
            Memo = node.Memo,
            Slug = node.Slug,
            Repository = node.Repository,
            Content = node.Content,
            HtmlContent = node.HtmlContent,
            PreviousDoc = currentIndex > 0 ? ToDocSummary(flatDocs[currentIndex - 1]) : null,
            NextDoc = currentIndex >= 0 && currentIndex < flatDocs.Count - 1 ? ToDocSummary(flatDocs[currentIndex + 1]) : null
        };

    private static DocNode ToDocSummary(DocNode node) =>
        new()
        {
            Name = node.Name,
            Memo = node.Memo,
            Slug = node.Slug,
            Repository = node.Repository
        };

    private static bool Contains(string? value, string keyword) =>
        value?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true;

    private static bool ContainsAny(IEnumerable<string>? values, string keyword) =>
        values?.Any(value => Contains(value, keyword)) == true;

    private static bool SlugEquals(string? left, string? right) =>
        string.Equals(Slugify(left), Slugify(right), StringComparison.OrdinalIgnoreCase);

    private static string NormalizeCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return SiteOptions.DefaultCultureName;
        }

        return culture.Trim().ToLowerInvariant() switch
        {
            "zh" or "zh-cn" or "zh-hans" or "zh_cn" => "zh-CN",
            "zh-tw" or "zh-hant" or "zh_tw" => "zh-TW",
            "ja" or "ja-jp" => "ja",
            "en" or "en-us" or "en-gb" => "en",
            _ => culture.Trim()
        };
    }

    private static string? CultureSuffix(string culture) =>
        NormalizeCulture(culture) switch
        {
            "zh-CN" => null,
            "zh-TW" => "zh-tw",
            "en" => "en",
            "ja" => "ja",
            _ => culture.ToLowerInvariant()
        };

    private static bool IsLocalizedSibling(string path) =>
        LocalizedSiblingRegex().IsMatch(Path.GetFileName(path));

    private static IEnumerable<ToolNode> FlattenTools(IEnumerable<ToolNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Hidden)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(node.Slug))
            {
                yield return node;
            }

            if (node.Children is null)
            {
                continue;
            }

            foreach (var child in FlattenTools(node.Children))
            {
                yield return child;
            }
        }
    }

    private static IEnumerable<DocNode> FlattenDocs(IEnumerable<DocNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (!string.IsNullOrWhiteSpace(node.Slug) && node.Children is not { Count: > 0 })
            {
                yield return node;
            }

            if (node.Children is null)
            {
                continue;
            }

            foreach (var child in FlattenDocs(node.Children))
            {
                yield return child;
            }
        }
    }

    private static (DocNode Node, string[] PathSegments)? FindDocNode(IEnumerable<DocNode> nodes, string slug, string[] parents)
    {
        foreach (var node in nodes)
        {
            if (SlugEquals(node.Slug, slug) && node.Children is not { Count: > 0 })
            {
                return (node, parents);
            }

            if (node.Children is not { Count: > 0 })
            {
                continue;
            }

            var nextParents = string.IsNullOrWhiteSpace(node.Slug)
                ? parents
                : [.. parents, node.Slug!];
            var found = FindDocNode(node.Children, slug, nextParents);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static SearchResultItem BuildSearchResult(BlogPost post, string keyword)
    {
        var content = post.Content ?? post.HtmlContent;
        var searchBody = string.Join(' ', new[]
        {
            post.Title,
            post.Description,
            post.Slug,
            string.Join(' ', post.Categories ?? []),
            string.Join(' ', post.Albums ?? []),
            string.Join(' ', post.Tags ?? []),
            content
        });
        var score = Score(keyword, post.Title, 80)
                    + Score(keyword, post.Description, 30)
                    + Score(keyword, post.Slug, 20)
                    + Score(keyword, string.Join(' ', post.Categories ?? []), 10)
                    + Score(keyword, string.Join(' ', post.Albums ?? []), 10)
                    + Score(keyword, string.Join(' ', post.Tags ?? []), 10)
                    + Score(keyword, content, 12);
        return new SearchResultItem
        {
            Kind = SearchResultKind.Post,
            Title = post.Title ?? post.Slug ?? "Post",
            Url = post.Url ?? "/post",
            Summary = post.Description,
            MatchedSnippet = CreateSnippet(searchBody, keyword),
            Context = string.Join(" / ", new[]
            {
                post.Categories?.FirstOrDefault(),
                post.Albums?.FirstOrDefault(),
                post.Tags?.FirstOrDefault()
            }.Where(static value => !string.IsNullOrWhiteSpace(value)).Select(static value => value!.Trim())),
            Slug = post.Slug,
            SourcePath = post.SourcePath,
            UpdatedAt = post.Lastmod ?? post.Date,
            Score = score
        };
    }

    private static SearchResultItem BuildSearchResult(ToolNode tool, string keyword)
    {
        var score = Score(keyword, tool.Name, 80) + Score(keyword, tool.Memo, 30) + Score(keyword, tool.Slug, 20);
        return new SearchResultItem
        {
            Kind = SearchResultKind.Tool,
            Title = tool.Name ?? tool.Slug ?? "Tool",
            Url = $"/tool/{tool.Slug}",
            Summary = tool.Memo,
            MatchedSnippet = CreateSnippet(string.Join(' ', new[] { tool.Name, tool.Memo, tool.Slug }.Where(static value => !string.IsNullOrWhiteSpace(value))), keyword),
            Context = "工具",
            Slug = tool.Slug,
            SourcePath = tool.Repository,
            UpdatedAt = null,
            Score = score
        };
    }

    private static SearchResultItem BuildSearchResult(DocNode doc, string keyword)
    {
        var score = Score(keyword, doc.Name, 80) + Score(keyword, doc.Memo, 30) + Score(keyword, doc.Slug, 20);
        return new SearchResultItem
        {
            Kind = SearchResultKind.Doc,
            Title = doc.Name ?? doc.Slug ?? "Doc",
            Url = $"/project/{doc.Slug}",
            Summary = doc.Memo,
            MatchedSnippet = CreateSnippet(string.Join(' ', new[] { doc.Name, doc.Memo, doc.Slug }.Where(static value => !string.IsNullOrWhiteSpace(value))), keyword),
            Context = "项目",
            Slug = doc.Slug,
            SourcePath = doc.Repository,
            UpdatedAt = null,
            Score = score
        };
    }

    private static SearchBlockedKeywordGroup? FindBlockedSearchKeywordGroup(string? keyword, IReadOnlyList<SearchBlockedKeywordGroup> groups)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return null;
        }

        foreach (var group in groups.OrderBy(static group => group.Sort))
        {
            if (group.Keywords is not { Count: > 0 })
            {
                continue;
            }

            if (group.Keywords.Any(item => !string.IsNullOrWhiteSpace(item) && keyword.Contains(item.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return group;
            }
        }

        return null;
    }

    private static string? CreateSnippet(string? value, string keyword, int radius = 24)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(keyword))
        {
            return null;
        }

        var text = WebUtility.HtmlDecode(Regex.Replace(value, "<[^>]+>", " "));
        var index = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var start = Math.Max(0, index - radius);
        var end = Math.Min(text.Length, index + keyword.Length + radius);
        var snippet = text[start..end].Trim();
        return $"{(start > 0 ? "…" : string.Empty)}{snippet}{(end < text.Length ? "…" : string.Empty)}";
    }

    private static int Score(string keyword, string? value, int weight)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (string.Equals(value, keyword, StringComparison.OrdinalIgnoreCase))
        {
            return weight * 3;
        }

        if (value.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
        {
            return weight * 2;
        }

        return value.Contains(keyword, StringComparison.OrdinalIgnoreCase) ? weight : 0;
    }

    private static string Slugify(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant();
        normalized = SlugInvalidCharacterRegex().Replace(normalized, "-");
        normalized = SlugDashRegex().Replace(normalized, "-");
        return normalized.Trim('-');
    }

    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string Xml(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    [GeneratedRegex(@"\.(en|ja|zh-tw)\.(md|yml|json)$", RegexOptions.IgnoreCase)]
    private static partial Regex LocalizedSiblingRegex();

    [GeneratedRegex(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled)]
    private static partial Regex SlugInvalidCharacterRegex();

    [GeneratedRegex("-+", RegexOptions.Compiled)]
    private static partial Regex SlugDashRegex();
}
