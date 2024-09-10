using CodeWF.Blog.Web.Client.Models;

namespace CodeWF.Blog.Web.Client.Services;

public interface IBlogPostService
{
    Task<IEnumerable<BlogPost>?> SearchAsync(string key);
    Task<BlogPost?> GetBySlug(string slug);
}