using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages;

public class SearchModel : PageModel
{
    private readonly AppService _appService;

    [BindProperty(SupportsGet = true, Name = "q")]
    public string Query { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public int PageSize { get; } = 10;
    public int Total { get; private set; }
    public int ToolCount { get; private set; }
    public int DocCount { get; private set; }
    public int PostCount { get; private set; }
    public List<SearchResultItem> Results { get; private set; } = [];
    public int TotalPages => Total <= 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

    public SearchModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        Query = Query?.Trim() ?? string.Empty;
        PageIndex = Math.Max(1, PageIndex);

        ViewData["Title"] = string.IsNullOrWhiteSpace(Query) ? "全局搜索" : $"搜索：{Query}";
        ViewData["Description"] = string.IsNullOrWhiteSpace(Query)
            ? "搜索站内的工具、文档和技术文章。"
            : $"查看与 {Query} 相关的工具、文档和技术文章搜索结果。";

        if (string.IsNullOrWhiteSpace(Query))
        {
            return;
        }

        var pageData = await _appService.SearchAsync(Query, PageIndex, PageSize);
        PageIndex = pageData.PageIndex;
        Results = pageData.Data;
        Total = pageData.Total;
        ToolCount = pageData.ToolCount;
        DocCount = pageData.DocCount;
        PostCount = pageData.PostCount;
    }
}
