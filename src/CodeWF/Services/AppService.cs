using System.Text;

namespace CodeWF.Services;

public class AppService(IOptions<SiteOption> siteOption)
{
    private List<DocItem>? _docItems;
    private List<ToolItem>? _toolItems;
    private List<AlbumItem>? _albumItems;
    private List<CategoryItem>? _categoryItems;
    private List<BlogPost>? _blogPosts;
    private List<FriendLinkItem>? _friendLinkItems;
    private List<TimeLineItem>? _TimeLineItems;
    private Dictionary<string, string>? _webSiteCountInfos;
    private string? _donationContent;
    private string? _aboutContent;
    private string? _rss;
    private string? _siteMap;

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

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "doc", "doc.json");
        if (!File.Exists(filePath))
        {
            return _docItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.FromJson(out _docItems, out var msg);
        return _docItems;
    }

    public async Task<List<ToolItem>?> GetAllToolItemsAsync()
    {
        if (_toolItems?.Any() == true)
        {
            return _toolItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "tools", "tools.json");
        if (!File.Exists(filePath))
        {
            return _toolItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.FromJson(out _toolItems, out var msg);
        return _toolItems;
    }

    public async Task<DocItem?> GetDocItemAsync(string slug)
    {
        async Task ReadContentAsync(DocItem item, string? parentDir = default)
        {
            if (!string.IsNullOrWhiteSpace(item.Content))
            {
                return;
            }

            var contentPath = string.Empty;
            contentPath = string.IsNullOrWhiteSpace(parentDir)
                ? Path.Combine(siteOption.Value.LocalAssetsDir, "site", "doc", $"{item.Slug}.md")
                : Path.Combine(siteOption.Value.LocalAssetsDir, "site", "doc", parentDir, $"{item.Slug}.md");

            if (File.Exists(contentPath))
            {
                item.Content = await File.ReadAllTextAsync(contentPath);
                item.HtmlContent = item.Content.ToHtml();
            }
        }

        if (_docItems?.Any() != true)
        {
            return default;
        }

        foreach (var item in _docItems)
        {
            if (item.Slug == slug)
            {
                await ReadContentAsync(item);
                return item;
            }

            if (item.Children?.Any() != true) continue;

            foreach (var itemChild in item.Children.Where(itemChild => itemChild.Slug == slug))
            {
                await ReadContentAsync(itemChild, item.Slug);
                return itemChild;
            }
        }

        var first = _docItems.FirstOrDefault();
        await ReadContentAsync(first);

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

    public async Task<List<AlbumItem>?> GetAllAlbumItemsAsync()
    {
        if (_albumItems?.Any() == true)
        {
            return _albumItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "album.json");
        if (!File.Exists(filePath))
        {
            return _albumItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.FromJson(out _albumItems, out var msg);
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

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "category.json");
        if (!File.Exists(filePath))
        {
            return _categoryItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.FromJson(out _categoryItems, out var msg);
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

        _blogPosts = new();
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
                var blogPost = await ReadBlogPostAsync(postFile);
                _blogPosts.Add(blogPost);
            }
        }

        return _blogPosts;
    }

    public async Task<PageData<BlogPost>?> GetPostByAlbum(int pageIndex, int pageSize, string albumSlug,
        string? key)
    {
        AlbumItem? album = null;
        if (!string.Equals(ConstantUtil.DefaultCategory, albumSlug))
        {
            album = _albumItems?.FirstOrDefault(albumDto => albumDto.Slug == albumSlug);
        }

        IEnumerable<BlogPost>? posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = _blogPosts
                ?.Where(p => p.Title?.Contains(key) == true
                             || p.Description?.Contains(key) == true
                             || p.Slug?.Contains(key) == true
                             || p.Author?.Contains(key) == true
                             || p.LastModifyUser?.Contains(key) == true
                             || p.Content?.Contains(key) == true);
        }
        else
        {
            posts = _blogPosts
                ?.Where(post => album == null || (post.Albums != null && post.Albums.Contains(album.Name) == true));
        }

        var total = posts.Count();

        var postDatas = posts
            .OrderBy(post => post.Date)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PageData<BlogPost>(pageIndex, pageSize, total, postDatas);
    }

    public async Task<PageData<BlogPost>?> GetPostByCategory(int pageIndex, int pageSize, string categorySlug,
        string? key)
    {
        CategoryItem? cat = null;
        if (!string.Equals(ConstantUtil.DefaultCategory, categorySlug))
        {
            cat = _categoryItems?.FirstOrDefault(cat => cat.Slug == categorySlug);
        }

        IEnumerable<BlogPost>? posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = _blogPosts
                ?.Where(p => p.Title?.Contains(key) == true
                             || p.Description?.Contains(key) == true
                             || p.Slug?.Contains(key) == true
                             || p.Author?.Contains(key) == true
                             || p.LastModifyUser?.Contains(key) == true
                             || p.Content?.Contains(key) == true);
        }
        else
        {
            posts = _blogPosts
                ?.Where(post => cat == null || (post.Categories != null && post.Categories.Contains(cat.Name) == true));
        }

        var total = posts.Count();

        var postDatas = posts
            .OrderByDescending(post => post.Lastmod)
            .ThenByDescending(post => post.Date)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PageData<BlogPost>(pageIndex, pageSize, total, postDatas);
    }

    public async Task<List<BlogPost>?> GetBannerPostAsync()
    {
        var posts = _blogPosts
            ?.Where(post => post.Banner)
            .OrderByDescending(post => post.Date)
            .ToList();
        return posts;
    }

    public async Task<Dictionary<string, string>> GetWebSiteCountAsync()
    {
        if (_webSiteCountInfos != null)
        {
            return _webSiteCountInfos;
        }

        _webSiteCountInfos = new();
        var total = _blogPosts?.Count;
        var original = _blogPosts?.Count(post => string.IsNullOrWhiteSpace(post.Author));
        var originalPercentage = (double)original / total * 100;
        _webSiteCountInfos["网站创建"] = $"{DateTime.Now.Year - siteOption.Value.StartYear}年";
        _webSiteCountInfos["文章分类"] = $"{_categoryItems?.Count}个";
        _webSiteCountInfos["文章总计"] = $"{total}篇";
        _webSiteCountInfos["文章原创"] = $"{original}篇({originalPercentage:F2}%)";
        return _webSiteCountInfos;
    }

    public async Task<string?> ReadAboutAsync()
    {
        if (!string.IsNullOrWhiteSpace(_aboutContent))
        {
            return _aboutContent;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "about.md");
        if (!File.Exists(filePath))
        {
            return "## 关于";
        }

        var content = await File.ReadAllTextAsync(filePath);
        _aboutContent = content.ToHtml();
        return _aboutContent;
    }

    public async Task<string?> ReadDonationAsync()
    {
        if (!string.IsNullOrWhiteSpace(_donationContent))
        {
            return _donationContent;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "pays", "Donation.md");
        if (!File.Exists(filePath))
        {
            return "## 赞助";
        }

        var content = await File.ReadAllTextAsync(filePath);
        _donationContent = content.ToHtml();
        return _donationContent;
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
            string a = ex.Message;
            Console.WriteLine(
                $"Blog post deserialize exception, file path is 【{markdownFilePath}】, exception information: {ex}");

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

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "FriendLink.json");
        if (!File.Exists(filePath))
        {
            return _friendLinkItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.FromJson(out _friendLinkItems, out var msg);
        return _friendLinkItems;
    }

    public async Task<List<TimeLineItem>?> GetTimeLineItemsAsync()
    {
        if (_TimeLineItems?.Any() == true)
        {
            return _TimeLineItems;
        }

        var filePath = Path.Combine(siteOption.Value.LocalAssetsDir, "site", "timelines.json");
        if (!File.Exists(filePath))
        {
            return _TimeLineItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.FromJson(out _TimeLineItems, out var msg);
        return _TimeLineItems;
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
                sb.Append($"<category>{string.Join(",", item.Categories)}</category>");
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