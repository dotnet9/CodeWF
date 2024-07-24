namespace CodeWF.Docs.Core.Services;

public class AppService
{
    public const int AppBarHeight = 96;
    public const int MobileAppBarHeight = 64;
    public const string ColorForNewState = "#05CD99";
    public const string ColorForUpdateState = "#FF5252";
    public const string ColorForBreakingChangeState = "#E040FB";
    public const string ColorForDeprecatedState = "#9E9E9E";

    private List<MarkdownItTocContent>? _toc;

    public event EventHandler<List<MarkdownItTocContent>?>? TocChanged;

    public List<MarkdownItTocContent>? Toc
    {
        get => _toc;
        set
        {
            _toc = value;
            TocChanged?.Invoke(this, value);
        }
    }

    public static List<DefaultItem> GetNavMenus(string? project)
    {
        var list = new List<DefaultItem>();

        list.Add(new("annual-service", "/annual-service", "pricing", "red"));
        
        return list;
    }

    public static List<DefaultItem> GetResources(string? project)
    {
        var list = new List<DefaultItem>();

        list.Add(new("blog", "https://codewf.com/post/"));
        list.Add(new("official-website", "https://codewf.com"));

        return list;
    }
}