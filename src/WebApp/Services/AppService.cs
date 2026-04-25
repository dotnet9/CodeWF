using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApp.Extensions;
using WebApp.Models;
using WebApp.Options;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebApp.Services;

public class AppService(IOptions<SiteOption> siteOption)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private List<DocItem>? _docItems;
    private List<ToolItem>? _toolItems;
    private List<AlbumItem>? _albumItems;
    private List<CategoryItem>? _categoryItems;
    private List<BlogPost>? _blogPosts;
    private List<FriendLinkItem>? _friendLinkItems;
    private List<TimeLineItem>? _timeLineItems;
    private Dictionary<string, string>? _webSiteCountInfos;
    private string? _donationMarkdown;
    private string? _donationHtmlContent;
    private string? _aboutMarkdown;
    private string? _aboutHtmlContent;
    private string? _rss;
    private string? _siteMap;

    private sealed record SearchField(string? Text, int ExactScore, int PrefixScore, int ContainsScore);
    private sealed record SearchableToolNode(ToolItem Item, string? GroupName);
    private sealed record SearchableDocNode(DocItem Item, string? ContextLabel, string? RelativeDir);

    public async Task SeedAsync()
    {
        await GetAllAlbumItemsAsync();
        await GetAllCategoryItemsAsync();
        await GetAllBlogPostsAsync();
        await GetAllFriendLinkItemsAsync();
        await GetTimeLineItemsAsync();
        await GetAllDocItemsAsync();
        await GetAllToolItemsAsync();
        await GetWebSiteCountAsync();
        await ReadAboutAsync();
        await ReadDonationAsync();
        await GetRssAsync();
        await GetSiteMapAsync();
    }

    public async Task<List<DocItem>?> GetAllDocItemsAsync()
    {
        if (_docItems?.Any() == true)
        {
            return _docItems;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return _docItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "doc", "doc.json");
        if (!File.Exists(filePath))
        {
            return _docItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _docItems = JsonSerializer.Deserialize<List<DocItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize doc.json: {ex.Message}");
        }
        return _docItems;
    }

    public async Task<List<ToolItem>?> GetAllToolItemsAsync()
    {
        if (_toolItems?.Any() == true)
        {
            return _toolItems;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return _toolItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "tools", "tools.json");
        if (!File.Exists(filePath))
        {
            return _toolItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _toolItems = JsonSerializer.Deserialize<List<ToolItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize tools.json: {ex.Message}");
        }
        return _toolItems;
    }

    public async Task<DocItem?> GetDocItemAsync(string slug)
    {
        if (_docItems?.Any() != true)
        {
            return default;
        }

        foreach (var item in _docItems)
        {
            if (!string.IsNullOrWhiteSpace(item.Slug) && item.Slug == slug)
            {
                await LoadDocContentAsync(item);
                return item;
            }

            if (item.Children?.Any() != true) continue;

            foreach (var itemChild in item.Children.Where(itemChild => itemChild.Slug == slug))
            {
                await LoadDocContentAsync(itemChild, item.Slug);
                return itemChild;
            }
        }

        var first = _docItems.FirstOrDefault()?.Children?.FirstOrDefault();
        if (first != null)
        {
            await LoadDocContentAsync(first);
        }

        return first;
    }

    public ToolItem? GetToolItem(string slug)
    {
        if (_toolItems?.Any() != true)
        {
            return default;
        }

        foreach (var item in _toolItems)
        {
            if (item.Children?.Any() != true) continue;

            foreach (var child in item.Children.Where(child =>
                         string.Equals(child.Slug, slug, StringComparison.InvariantCultureIgnoreCase)))
            {
                return child;
            }
        }

        return default;
    }

    public async Task<SearchResultPageData> SearchAsync(string? query, int pageIndex, int pageSize, SearchResultKind? kind = null)
    {
        var normalizedQuery = query?.Trim() ?? string.Empty;
        var safePageIndex = Math.Max(1, pageIndex);
        var safePageSize = pageSize <= 0 ? 10 : pageSize;

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return new SearchResultPageData(safePageIndex, safePageSize, 0, [], 0, 0, 0);
        }

        await GetAllBlogPostsAsync();
        await GetAllDocItemsAsync();
        await GetAllToolItemsAsync();

        var tokens = SplitSearchTokens(normalizedQuery);
        if (tokens.Count == 0)
        {
            return new SearchResultPageData(safePageIndex, safePageSize, 0, [], 0, 0, 0);
        }

        var results = new List<SearchResultItem>();

        foreach (var tool in GetSearchableToolNodes())
        {
            var score = CalculateSearchScore(
                normalizedQuery,
                tokens,
                new SearchField(tool.Item.Name, 220, 170, 130),
                new SearchField(tool.Item.Slug, 180, 140, 100),
                new SearchField(tool.Item.Memo, 90, 60, 36),
                new SearchField(tool.GroupName, 70, 45, 24));

            if (score <= 0)
            {
                continue;
            }

            var summary = SelectSummary(normalizedQuery, tool.Item.Memo, tool.GroupName);
            var matchedSnippet = SelectSnippet(normalizedQuery, tool.Item.Memo, tool.GroupName);

            results.Add(new SearchResultItem
            {
                Kind = SearchResultKind.Tool,
                Title = tool.Item.Name?.Trim() ?? "未命名工具",
                Url = ConstantUtil.GetToolUrl(tool.Item.Slug),
                Summary = summary,
                MatchedSnippet = matchedSnippet,
                Context = tool.GroupName,
                Slug = tool.Item.Slug,
                Score = score
            });
        }

        var searchableDocs = await GetSearchableDocNodesAsync();
        foreach (var doc in searchableDocs)
        {
            var plainContent = CleanSearchText(doc.Item.Content);
            var score = CalculateSearchScore(
                normalizedQuery,
                tokens,
                new SearchField(doc.Item.Name, 220, 170, 130),
                new SearchField(doc.Item.Slug, 180, 140, 100),
                new SearchField(doc.Item.Memo, 90, 60, 36),
                new SearchField(doc.ContextLabel, 70, 45, 24),
                new SearchField(plainContent, 26, 18, 12));

            if (score <= 0)
            {
                continue;
            }

            var summary = SelectSummary(normalizedQuery, doc.Item.Memo, plainContent);
            var matchedSnippet = SelectSnippet(normalizedQuery, doc.Item.Memo, plainContent);

            results.Add(new SearchResultItem
            {
                Kind = SearchResultKind.Doc,
                Title = doc.Item.Name?.Trim() ?? "未命名文档",
                Url = ConstantUtil.GetDocUrl(doc.Item.Slug),
                Summary = summary,
                MatchedSnippet = matchedSnippet,
                Context = doc.ContextLabel,
                Slug = doc.Item.Slug,
                Score = score
            });
        }

        foreach (var post in _blogPosts ?? [])
        {
            var plainContent = CleanSearchText(post.Content);
            var categoryText = string.Join(" / ", post.Categories?.Where(static item => !string.IsNullOrWhiteSpace(item)) ?? []);
            var albumText = string.Join(" / ", post.Albums?.Where(static item => !string.IsNullOrWhiteSpace(item)) ?? []);
            var tagText = string.Join(" / ", post.Tags?.Where(static item => !string.IsNullOrWhiteSpace(item)) ?? []);
            var score = CalculateSearchScore(
                normalizedQuery,
                tokens,
                new SearchField(post.Title, 230, 180, 135),
                new SearchField(post.Slug, 185, 145, 105),
                new SearchField(post.Description, 95, 65, 40),
                new SearchField(categoryText, 75, 48, 28),
                new SearchField(albumText, 60, 42, 24),
                new SearchField(tagText, 60, 42, 24),
                new SearchField(post.Author, 48, 36, 22),
                new SearchField(plainContent, 28, 20, 14));

            if (score <= 0)
            {
                continue;
            }

            var summary = SelectSummary(normalizedQuery, post.Description, plainContent);
            var matchedSnippet = SelectSnippet(normalizedQuery, post.Description, plainContent);
            var context = !string.IsNullOrWhiteSpace(categoryText)
                ? categoryText
                : !string.IsNullOrWhiteSpace(albumText)
                    ? albumText
                    : null;

            results.Add(new SearchResultItem
            {
                Kind = SearchResultKind.Post,
                Title = post.Title?.Trim() ?? "未命名文章",
                Url = ConstantUtil.GetBbsPostUrl(post),
                Summary = summary,
                MatchedSnippet = matchedSnippet,
                Context = context,
                Slug = post.Slug,
                UpdatedAt = post.Lastmod ?? post.Date,
                Score = score
            });
        }

        var ordered = results
            .OrderBy(result => result.Kind)
            .ThenByDescending(result => result.Score)
            .ThenByDescending(result => result.UpdatedAt ?? DateTime.MinValue)
            .ThenBy(result => result.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var filtered = kind.HasValue
            ? ordered.Where(result => result.Kind == kind.Value).ToList()
            : ordered;

        var totalCount = filtered.Count;
        var totalPages = totalCount <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)safePageSize);
        var currentPageIndex = totalPages <= 0
            ? 1
            : Math.Min(safePageIndex, totalPages);

        var data = filtered
            .Skip((currentPageIndex - 1) * safePageSize)
            .Take(safePageSize)
            .ToList();

        return new SearchResultPageData(
            currentPageIndex,
            safePageSize,
            totalCount,
            data,
            ordered.Count(result => result.Kind == SearchResultKind.Tool),
            ordered.Count(result => result.Kind == SearchResultKind.Doc),
            ordered.Count(result => result.Kind == SearchResultKind.Post));
    }

    private async Task LoadDocContentAsync(DocItem item, string? parentDir = default)
    {
        if (!string.IsNullOrWhiteSpace(item.Content))
        {
            return;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir) || string.IsNullOrWhiteSpace(item.Slug))
        {
            return;
        }

        var contentPath = string.IsNullOrWhiteSpace(parentDir)
            ? Path.Combine(siteOption.Value.LocalAssetsDir, "site", "doc", $"{item.Slug}.md")
            : Path.Combine(siteOption.Value.LocalAssetsDir, "site", "doc", parentDir, $"{item.Slug}.md");

        if (!File.Exists(contentPath))
        {
            return;
        }

        item.Content = await File.ReadAllTextAsync(contentPath);
        item.HtmlContent = item.Content.ToHtml();
    }

    private IEnumerable<SearchableToolNode> GetSearchableToolNodes()
    {
        if (_toolItems?.Any() != true)
        {
            yield break;
        }

        foreach (var item in _toolItems)
        {
            if (item.Children?.Any() == true)
            {
                foreach (var child in item.Children.Where(static child =>
                             !string.IsNullOrWhiteSpace(child.Name) || !string.IsNullOrWhiteSpace(child.Slug)))
                {
                    yield return new SearchableToolNode(child, item.Name);
                }

                continue;
            }

            if (!string.IsNullOrWhiteSpace(item.Name) || !string.IsNullOrWhiteSpace(item.Slug))
            {
                yield return new SearchableToolNode(item, null);
            }
        }
    }

    private async Task<List<SearchableDocNode>> GetSearchableDocNodesAsync()
    {
        var nodes = new List<SearchableDocNode>();
        if (_docItems?.Any() != true)
        {
            return nodes;
        }

        async Task TraverseAsync(IEnumerable<DocItem> items, string? parentLabel = null, string? parentDir = null)
        {
            foreach (var item in items)
            {
                await LoadDocContentAsync(item, parentDir);
                nodes.Add(new SearchableDocNode(item, parentLabel, parentDir));

                if (item.Children?.Any() != true || string.IsNullOrWhiteSpace(item.Slug))
                {
                    continue;
                }

                var nextParentLabel = string.IsNullOrWhiteSpace(parentLabel)
                    ? item.Name
                    : $"{parentLabel} / {item.Name}";
                var nextParentDir = string.IsNullOrWhiteSpace(parentDir)
                    ? item.Slug
                    : Path.Combine(parentDir, item.Slug);

                await TraverseAsync(item.Children, nextParentLabel, nextParentDir);
            }
        }

        await TraverseAsync(_docItems);
        return nodes;
    }

    private static int CalculateSearchScore(string query, IReadOnlyCollection<string> tokens, params SearchField[] fields)
    {
        if (tokens.Count == 0 || fields.Length == 0)
        {
            return 0;
        }

        var totalScore = 0;

        foreach (var token in tokens)
        {
            var tokenScore = 0;
            foreach (var field in fields)
            {
                tokenScore = Math.Max(tokenScore,
                    GetMatchScore(field.Text, token, field.ExactScore, field.PrefixScore, field.ContainsScore));
            }

            if (tokenScore <= 0)
            {
                return 0;
            }

            totalScore += tokenScore;
        }

        foreach (var field in fields)
        {
            totalScore += GetFullQueryBonus(field.Text, query, field.ExactScore, field.PrefixScore, field.ContainsScore);
        }

        return totalScore;
    }

    private static int GetMatchScore(string? source, string token, int exactScore, int prefixScore, int containsScore)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return 0;
        }

        var text = source.Trim();
        if (text.Equals(token, StringComparison.OrdinalIgnoreCase))
        {
            return exactScore;
        }

        if (text.StartsWith(token, StringComparison.OrdinalIgnoreCase))
        {
            return prefixScore;
        }

        return text.Contains(token, StringComparison.OrdinalIgnoreCase) ? containsScore : 0;
    }

    private static int GetFullQueryBonus(string? source, string query, int exactScore, int prefixScore, int containsScore)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(query))
        {
            return 0;
        }

        var text = source.Trim();
        if (text.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return exactScore / 2;
        }

        if (text.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        {
            return prefixScore / 2;
        }

        return text.Contains(query, StringComparison.OrdinalIgnoreCase) ? Math.Max(containsScore / 2, 1) : 0;
    }

    private static List<string> SplitSearchTokens(string? query) =>
        (query ?? string.Empty)
        .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static string? SelectSummary(string query, params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var snippet = SelectSnippet(query, candidate);
            if (!string.IsNullOrWhiteSpace(snippet))
            {
                return snippet;
            }
        }

        return null;
    }

    private static string? SelectSnippet(string query, params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var snippet = SelectSnippet(query, candidate, 130);
            if (!string.IsNullOrWhiteSpace(snippet))
            {
                return snippet;
            }
        }

        return null;
    }

    private static string? SelectSnippet(string query, string? source, int maxLength = 130)
    {
        var cleaned = CleanSearchText(source);
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return null;
        }

        if (cleaned.Length <= maxLength)
        {
            return cleaned;
        }

        var index = cleaned.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            foreach (var token in SplitSearchTokens(query))
            {
                index = cleaned.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    break;
                }
            }
        }

        if (index < 0)
        {
            return $"{cleaned[..maxLength].Trim()}...";
        }

        var start = Math.Max(0, index - (maxLength / 3));
        var length = Math.Min(maxLength, cleaned.Length - start);
        var snippet = cleaned.Substring(start, length).Trim();

        if (start > 0)
        {
            snippet = $"...{snippet}";
        }

        if (start + length < cleaned.Length)
        {
            snippet = $"{snippet}...";
        }

        return snippet;
    }

    private static string CleanSearchText(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        var cleaned = source;
        cleaned = Regex.Replace(cleaned, "<[^>]+>", " ");
        cleaned = Regex.Replace(cleaned, @"!\[[^\]]*\]\([^)]+\)", " ");
        cleaned = Regex.Replace(cleaned, @"\[(.*?)\]\([^)]+\)", "$1");
        cleaned = Regex.Replace(cleaned, @"[`*_>#\-]+", " ");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        return cleaned.Trim();
    }

    public async Task<List<AlbumItem>?> GetAllAlbumItemsAsync()
    {
        if (_albumItems?.Any() == true)
        {
            return _albumItems;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return _albumItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "album.json");
        if (!File.Exists(filePath))
        {
            return _albumItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _albumItems = JsonSerializer.Deserialize<List<AlbumItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize album.json: {ex.Message}");
        }
        
        if (_albumItems == null)
        {
            _albumItems = new List<AlbumItem>();
        }

        _albumItems.Insert(0, new AlbumItem() { Slug = ConstantUtil.DefaultCategory, Name = "所有" });
        return _albumItems;
    }

    public async Task<List<CategoryItem>?> GetAllCategoryItemsAsync()
    {
        if (_categoryItems?.Any() == true)
        {
            return _categoryItems;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return _categoryItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "category.json");
        if (!File.Exists(filePath))
        {
            return _categoryItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _categoryItems = JsonSerializer.Deserialize<List<CategoryItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize category.json: {ex.Message}");
        }
        
        if (_categoryItems == null)
        {
            _categoryItems = new List<CategoryItem>();
        }

        _categoryItems.Insert(0, new CategoryItem() { Slug = ConstantUtil.DefaultCategory, Name = "所有" });
        return _categoryItems;
    }


    public async Task<List<BlogPost>?> GetAllBlogPostsAsync()
    {
        if (_blogPosts?.Any() == true)
        {
            return _blogPosts;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            _blogPosts = new List<BlogPost>();
            return _blogPosts;
        }

        _blogPosts = new List<BlogPost>();
        var endYear = DateTime.Now.Year;

        for (var start = siteOption.Value.StartYear; start <= endYear; start++)
        {
            var postDir = Path.Combine(siteOption.Value.LocalAssetsDir, start.ToString());
            if (!Directory.Exists(postDir))
            {
                continue;
            }

            var postFiles = Directory.GetFiles(postDir, "*.md", SearchOption.AllDirectories);
            foreach (var postFile in postFiles)
            {
                try
                {
                    var blogPost = await ReadBlogPostAsync(postFile);
                    if (!blogPost.Draft)
                    {
                        _blogPosts.Add(blogPost);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read blog post {postFile}: {ex.Message}");
                }
            }
        }

        _blogPosts = _blogPosts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue)
            .ToList();

        return _blogPosts;
    }

    public Task<PageData<BlogPost>> GetPostByAlbum(int pageIndex, int pageSize, string albumSlug,
        string? key)
    {
        AlbumItem? album = null;
        if (!string.Equals(ConstantUtil.DefaultCategory, albumSlug))
        {
            album = _albumItems?.FirstOrDefault(albumDto => albumDto.Slug == albumSlug);
        }

        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = (_blogPosts ?? [])
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.LastModifyUser?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }
        else
        {
            posts = (_blogPosts ?? [])
                .Where(post => album == null
                               || (!string.IsNullOrWhiteSpace(album.Name)
                                   && post.Albums?.Contains(album.Name, StringComparer.OrdinalIgnoreCase) == true));
        }

        var ordered = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var postDatas = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(new PageData<BlogPost>(pageIndex, pageSize, total, postDatas));
    }

    public Task<PageData<BlogPost>> GetPostByCategory(int pageIndex, int pageSize, string categorySlug,
        string? key)
    {
        CategoryItem? cat = null;
        if (!string.Equals(ConstantUtil.DefaultCategory, categorySlug))
        {
            cat = _categoryItems?.FirstOrDefault(cat => cat.Slug == categorySlug);
        }

        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = (_blogPosts ?? [])
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.LastModifyUser?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }
        else
        {
            posts = (_blogPosts ?? [])
                .Where(post => cat == null
                               || (!string.IsNullOrWhiteSpace(cat.Name)
                                   && post.Categories?.Contains(cat.Name, StringComparer.OrdinalIgnoreCase) == true));
        }

        var ordered = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var postDatas = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(new PageData<BlogPost>(pageIndex, pageSize, total, postDatas));
    }

    public Task<List<BlogPost>?> GetBannerPostAsync()
    {
        var posts = _blogPosts
            ?.Where(post => post.Banner)
            .OrderByDescending(post => post.Date)
            .ToList();
        return Task.FromResult(posts);
    }

    public Task<PageData<BlogPost>> GetPagedBlogPostsAsync(int pageIndex, int pageSize, string? key = null)
    {
        var source = _blogPosts?.AsEnumerable() ?? [];

        if (!string.IsNullOrWhiteSpace(key))
        {
            source = source.Where(post =>
                post.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }

        var ordered = source
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var data = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PageData<BlogPost>(pageIndex, pageSize, total, data));
    }

    public Task<Dictionary<string, string>> GetWebSiteCountAsync()
    {
        if (_webSiteCountInfos != null)
        {
            return Task.FromResult(_webSiteCountInfos);
        }

        _webSiteCountInfos = new();
        var total = _blogPosts?.Count ?? 0;
        var original = _blogPosts?.Count(post => string.IsNullOrWhiteSpace(post.Author)) ?? 0;
        var originalPercentage = total > 0 ? (double)original / total * 100 : 0;
        _webSiteCountInfos["网站创建"] = $"{DateTime.Now.Year - siteOption.Value.StartYear}年";
        _webSiteCountInfos["文章分类"] = $"{_categoryItems?.Count}个";
        _webSiteCountInfos["文章总计"] = $"{total}篇";
        _webSiteCountInfos["文章原创"] = $"{original}篇({originalPercentage:F2}%)";
        return Task.FromResult(_webSiteCountInfos);
    }

    public async Task<(string? Markdown, string? HtmlContent)> ReadAboutAsync()
    {
        if (!string.IsNullOrWhiteSpace(_aboutMarkdown))
        {
            return (_aboutMarkdown, _aboutHtmlContent);
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return ("## 关于", "关于");
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "about.md");
        if (!File.Exists(filePath))
        {
            return ("## 关于", "关于");
        }

        _aboutMarkdown = await File.ReadAllTextAsync(filePath);
        _aboutHtmlContent = _aboutMarkdown.ToHtml();
        return (_aboutMarkdown, _aboutHtmlContent);
    }

    public async Task<(string? Markdown, string? HtmlContent)> ReadDonationAsync()
    {
        if (!string.IsNullOrWhiteSpace(_donationMarkdown))
        {
            return (_donationMarkdown, _donationHtmlContent);
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return ("## 赞助", "赞助");
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "pays", "Donation.md");
        if (!File.Exists(filePath))
        {
            return ("## 赞助", "赞助");
        }

        _donationMarkdown = await File.ReadAllTextAsync(filePath);
        _donationHtmlContent = _donationMarkdown.ToHtml();
        return (_donationMarkdown, _donationHtmlContent);
    }

    public async Task<BlogPost?> GetPostBySlug(string slug)
    {
        var post = _blogPosts?.FirstOrDefault(cat => cat.Slug == slug);
        return post;
    }

    public static async Task<BlogPost> ReadBlogPostAsync(string markdownFilePath)
    {
        var markdown = await File.ReadAllTextAsync(markdownFilePath);
        var endOfFrontMatter = markdown.IndexOf("---", 3, StringComparison.Ordinal);
        if (endOfFrontMatter == -1)
        {
            throw new InvalidOperationException("Invalid markdown format. No ending '---' found for Front Matter.");
        }

        var frontMatterText = markdown[..(endOfFrontMatter + 3)];
        var markdownContent = markdown[(endOfFrontMatter + 3)..].Trim();
        if (frontMatterText.StartsWith("---"))
        {
            frontMatterText = frontMatterText[3..].Trim();
        }

        if (frontMatterText.EndsWith("---"))
        {
            frontMatterText = frontMatterText[..^3].Trim();
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        BlogPost blogPost;
        try
        {
            blogPost = deserializer.Deserialize<BlogPost>(frontMatterText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize blog post front matter: {markdownFilePath}. {ex.Message}");

            blogPost = new BlogPost();
        }

        blogPost.Content = markdownContent;
        blogPost.HtmlContent = markdownContent.ToHtml();

        return blogPost;
    }

    public async Task<List<FriendLinkItem>?> GetAllFriendLinkItemsAsync()
    {
        if (_friendLinkItems?.Any() == true)
        {
            return _friendLinkItems;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return _friendLinkItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "FriendLink.json");
        if (!File.Exists(filePath))
        {
            return _friendLinkItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _friendLinkItems = JsonSerializer.Deserialize<List<FriendLinkItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize FriendLink.json: {ex.Message}");
        }
        return _friendLinkItems;
    }

    public async Task<List<TimeLineItem>?> GetTimeLineItemsAsync()
    {
        if (_timeLineItems?.Any() == true)
        {
            return _timeLineItems;
        }

        if (string.IsNullOrEmpty(siteOption.Value.LocalAssetsDir))
        {
            return _timeLineItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "timelines.json");
        if (!File.Exists(filePath))
        {
            return _timeLineItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _timeLineItems = JsonSerializer.Deserialize<List<TimeLineItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize timelines.json: {ex.Message}");
        }
        return _timeLineItems;
    }

    public async Task<string> GetRssAsync()
    {
        if (!string.IsNullOrWhiteSpace(_rss))
        {
            return _rss;
        }

        var data = _blogPosts?
            .OrderByDescending(p => p.Lastmod)
            .ThenByDescending(p => p.Date)
            .Take(10)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine(
            "<rss xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:content=\"http://purl.org/rss/1.0/modules/content/\" version=\"2.0\">");
        sb.Append("<channel>");
        sb.Append(
            $"<atom:link rel=\"self\" type=\"application/rss+xml\" href=\"{siteOption.Value.Domain}/rss\"/>");
        sb.Append($"<title>{siteOption.Value.AppTitle}_{siteOption.Value.Memo}</title>");
        sb.Append($"<link>{siteOption.Value.Domain}/rss</link>");
        sb.Append($"<description>{siteOption.Value.Memo}</description>");
        sb.Append($"<copyright>{siteOption.Value.AppTitle}_{siteOption.Value.Memo}</copyright>");
        sb.Append("<language>zh-cn</language>");
        if (data is { Count: > 0 })
        {
            foreach (var item in data)
            {
                sb.Append("<item>");
                sb.Append($"<title>{item.Title}</title>");
                sb.Append(
                    $"<link>{siteOption.Value.Domain}/bbs/post/{item.Date?.ToString("yyyy/MM")}/{item.Slug}</link>");
                sb.Append($"<description>{item.Description}</description>");
                sb.Append($"<author>({item.Author ?? siteOption.Value.Owner})</author>");
                sb.Append($"<category>{string.Join(",", item.Categories ?? [])}</category>");
                sb.Append(
                    $"<guid>{siteOption.Value.Domain}/{item.Date?.ToString("yyyy/MM")}/{item.Slug}</guid>");
                sb.Append($"<pubDate>{item.Date:R}</pubDate>");
                sb.Append($"<content:encoded><![CDATA[{item.Description}]]></content:encoded>");
                sb.Append("</item>");
            }
        }

        sb.Append("</channel>");
        sb.AppendLine("</rss>");

        _rss = sb.ToString();

        return _rss;
    }

    public async Task<string> GetSiteMapAsync()
    {
        if (!string.IsNullOrWhiteSpace(_siteMap))
        {
            return _siteMap;
        }

        List<SitemapNode> siteMapNodes = new();

        if (_categoryItems?.Any() == true)
        {
            siteMapNodes.AddRange(_categoryItems.Select(x => new SitemapNode
            {
                LastModified = DateTimeOffset.UtcNow,
                Priority = 0.8,
                Url = $"{siteOption.Value.Domain}/bbs/cat/{x.Slug}",
                Frequency = SitemapFrequency.Monthly
            }));
        }

        if (_blogPosts?.Any() == true)
        {
            siteMapNodes.AddRange(_blogPosts
                .OrderByDescending(p => p.Lastmod)
                .ThenByDescending(p => p.Date)
                .Select(x =>
                    new SitemapNode
                    {
                        LastModified = x.Lastmod ?? x.Date ?? DateTimeOffset.Now,
                        Priority = 0.9,
                        Url =
                            $"{siteOption.Value.Domain}/bbs/post/{x.Date:yyyy/MM}/{x.Slug}",
                        Frequency = SitemapFrequency.Daily
                    }));
        }

        StringBuilder sb = new();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"");
        sb.AppendLine("   xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
        sb.AppendLine(
            "   xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\">");

        foreach (SitemapNode m in siteMapNodes)
        {
            sb.AppendLine("    <url>");

            sb.AppendLine($"        <loc>{m.Url}</loc>");
            sb.AppendLine($"        <lastmod>{m.LastModified.ToString("yyyy-MM-dd")}</lastmod>");
            sb.AppendLine($"        <changefreq>{m.Frequency}</changefreq>");
            sb.AppendLine($"        <priority>{m.Priority}</priority>");

            sb.AppendLine("    </url>");
        }

        sb.AppendLine("</urlset>");

        _siteMap = sb.ToString();

        return _siteMap;
    }
}
