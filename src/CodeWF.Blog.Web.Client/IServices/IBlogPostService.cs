using CodeWF.Blog.Web.Client.Models.BlogPosts;

namespace CodeWF.Blog.Web.Client.IServices;

public interface IBlogPostService
{
    Task<List<BlogPost>?> GetBannersAsync();
    Task<List<BlogPost>?> SearchAsync(string key);
    Task<BlogPost?> GetBySlug(string slug);
}