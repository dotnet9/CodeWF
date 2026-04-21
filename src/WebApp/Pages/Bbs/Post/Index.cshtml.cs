using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Bbs.Post;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public BlogPost? Post { get; set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(int year, int month, string slug)
    {
        // 这里需要根据year、month和slug获取文章
        // 暂时使用slug直接查询
        Post = await _appService.GetPostBySlug(slug);
    }
}