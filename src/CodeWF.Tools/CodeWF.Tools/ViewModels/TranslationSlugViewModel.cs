using System.Reactive;

namespace CodeWF.Tools.ViewModels;

internal sealed class TranslationSlugViewModel : ViewModelBase
{
    private readonly ITranslationService? _translationService = Locator.Current.GetService<ITranslationService>();
    private TranslationKind _kind = TranslationKind.ChineseToSlug;

    /// <summary>
    /// 中文标题
    /// </summary>
    public TranslationKind Kind
    {
        get => _kind;
        set
        {
            if (value != _kind)
            {
                this.RaiseAndSetIfChanged(ref _kind, value);
            }
        }
    }

    private string? _from;

    /// <summary>
    /// 待翻译字符串
    /// </summary>
    public string? From
    {
        get => _from;
        set
        {
            if (value != _from)
            {
                this.RaiseAndSetIfChanged(ref _from, value);
                if (_isAutoTranslation)
                {
                    HandleTranslationAsync().WaitAsync(TimeSpan.FromSeconds(3));
                }
            }
        }
    }

    private string? _to;

    /// <summary>
    /// 目标翻译字符串
    /// </summary>
    public string? To
    {
        get => _to;
        set
        {
            if (value != _to)
            {
                this.RaiseAndSetIfChanged(ref _to, value);
            }
        }
    }

    private bool _isAutoTranslation = true;

    /// <summary>
    /// 自动翻译
    /// </summary>
    public bool IsAutoTranslation
    {
        get => _isAutoTranslation;
        set
        {
            if (value != _isAutoTranslation)
            {
                this.RaiseAndSetIfChanged(ref _isAutoTranslation, value);
            }
        }
    }

    public ReactiveCommand<TranslationKind, Unit> KindChanged { get; }

    public TranslationSlugViewModel()
    {
        KindChanged = ReactiveCommand.Create<TranslationKind>(OnKindChanged);
    }

    public async Task HandleTranslationAsync()
    {
        if (string.IsNullOrWhiteSpace(From))
        {
            To = string.Empty;
            return;
        }

        try
        {
            switch (Kind)
            {
                case TranslationKind.ChineseToEnglish:
                    To = await _translationService!.ChineseToEnglishAsync(From);
                    break;
                case TranslationKind.EnglishToChinese:
                    To = await _translationService!.EnglishToChineseAsync(From);
                    break;
                case TranslationKind.ChineseToSlug:
                    var english = await _translationService!.ChineseToEnglishAsync(From);
                    To = _translationService!.EnglishToUrlSlug(english);
                    break;
                default:
                    To = _translationService!.EnglishToUrlSlug(From);
                    break;
            }
        }
        catch (Exception ex)
        {
            To = ex.Message;
            SnackbarHost.Post($"翻译异常，请联系作者：{ex.Message}");
        }
    }

    private void OnKindChanged(TranslationKind newKind)
    {
        Kind = newKind;
    }
}