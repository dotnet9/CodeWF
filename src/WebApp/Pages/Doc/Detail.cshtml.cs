using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Doc;

public class DetailModel : PageModel
{
    private readonly AppService _appService;

    public DocItem? Doc { get; set; }
    public List<DocItem> DocItems { get; set; } = [];
    public DocItem? PreviousDoc { get; private set; }
    public DocItem? NextDoc { get; private set; }

    public DetailModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(string slug)
    {
        DocItems = await _appService.GetAllDocItemsAsync() ?? [];
        Doc = await _appService.GetDocItemAsync(slug);
        if (Doc == null)
        {
            return;
        }

        var flatDocs = FlattenDocs(DocItems)
            .Where(static item => !string.IsNullOrWhiteSpace(item.Slug))
            .ToList();

        var currentIndex = flatDocs.FindIndex(item =>
            string.Equals(item.Slug, Doc.Slug, StringComparison.OrdinalIgnoreCase));

        if (currentIndex > 0)
        {
            PreviousDoc = flatDocs[currentIndex - 1];
        }

        if (currentIndex >= 0 && currentIndex < flatDocs.Count - 1)
        {
            NextDoc = flatDocs[currentIndex + 1];
        }
    }

    private static IEnumerable<DocItem> FlattenDocs(IEnumerable<DocItem> items)
    {
        foreach (var item in items)
        {
            if (item.Children?.Any() == true)
            {
                foreach (var child in item.Children)
                {
                    yield return child;
                }

                continue;
            }

            yield return item;
        }
    }
}
