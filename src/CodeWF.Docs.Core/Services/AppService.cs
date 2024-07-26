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
}