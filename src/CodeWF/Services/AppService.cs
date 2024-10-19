using CodeWF.Options;
using CodeWF.Tools.Extensions;
using Microsoft.Extensions.Options;

namespace CodeWF.Services;

public class AppService
{
    private readonly SiteOption _siteInfo;
    private List<DocItem>? _docItems;

    public AppService(IOptions<SiteOption> siteOption)
    {
        _siteInfo = siteOption.Value;
    }

    public async Task<List<DocItem>?> GetAllDocItemsAsync()
    {
        if (_docItems?.Any() == true)
        {
            return _docItems;
        }

        var filePath = Path.Combine(_siteInfo.LocalAssetsDir, "site", "doc", "doc.json");
        if (!File.Exists(filePath))
        {
            return _docItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.FromJson(out _docItems, out var msg);
        return _docItems;
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
                ? Path.Combine(_siteInfo.LocalAssetsDir, "site", "doc", $"{item.Slug}.md")
                : Path.Combine(_siteInfo.LocalAssetsDir, "site", "doc", parentDir, $"{item.Slug}.md");

            if (File.Exists(contentPath))
            {
                item.Content = await File.ReadAllTextAsync(contentPath);
            }
        }

        var items = await GetAllDocItemsAsync();
        foreach (var item in items)
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

        var first = items.FirstOrDefault();
        await ReadContentAsync(first);

        return first;
    }
}