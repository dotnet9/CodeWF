using CodeWF.Blog.Web.Client.Helpers;
using CodeWF.Blog.Web.Client.Models;
using CodeWF.Blog.Web.Client.Options;
using Microsoft.Extensions.Options;

namespace CodeWF.Blog.Web.Client.Services;

public class BlogPostService(IOptions<SiteOption> site) : IBlogPostService
{
    private static List<BlogPost>? _allBlogPosts;

    public async Task<IEnumerable<BlogPost>?> SearchAsync(string key)
    {
        _allBlogPosts ??= await AssetsHelper.ReadBlogPostsAsync(site.Value.LocalAssetsDir!);

        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return _allBlogPosts
            .Where(item => item.Title!.Contains(key, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Title);
    }

    public async Task<BlogPost?> GetBySlug(string slug)
    {
        return _allBlogPosts?.FirstOrDefault(post => post.Slug == slug);
    }
}