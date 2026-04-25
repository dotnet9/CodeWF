using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Bbs.Post;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public BlogPost? Post { get; set; }
    public BlogPost? PreviousPost { get; private set; }
    public BlogPost? NextPost { get; private set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(int year, int month, string slug)
    {
        Post = await _appService.GetPostBySlug(slug);
        if (Post == null)
        {
            return;
        }

        var posts = await _appService.GetAllBlogPostsAsync() ?? [];
        var currentIndex = posts.FindIndex(item =>
            string.Equals(item.Slug, Post.Slug, StringComparison.OrdinalIgnoreCase));

        if (currentIndex > 0)
        {
            PreviousPost = posts[currentIndex - 1];
        }

        if (currentIndex >= 0 && currentIndex < posts.Count - 1)
        {
            NextPost = posts[currentIndex + 1];
        }
    }
}
