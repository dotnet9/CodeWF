using Unit = System.Reactive.Unit;

namespace CodeWF.Tools.Modules.Web.ViewModels;

public class SlugifyViewModel : ViewModelBase
{
    private readonly INotificationService? _notificationService;
    private readonly IClipboardService? _clipboardService;
    private readonly ITranslationService? _translationService;
    private readonly ISender _sender;
    private readonly IPublisher _publisher;
    private readonly IEventAggregator _eventAggregator;
    private TranslationKind _kind = TranslationKind.ChineseToSlug;

    /// <summary>
    /// 中文标题
    /// </summary>
    public TranslationKind Kind
    {
        get => _kind;
        set => SetProperty(ref _kind, value);
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
                SetProperty(ref _from, value);
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
        set => SetProperty(ref _to, value);
    }

    private bool _isAutoTranslation = true;

    /// <summary>
    /// 自动翻译
    /// </summary>
    public bool IsAutoTranslation
    {
        get => _isAutoTranslation;
        set => SetProperty(ref _isAutoTranslation, value);
    }

    public ReactiveCommand<TranslationKind, Unit> KindChanged { get; }

    public SlugifyViewModel(INotificationService notificationService, IClipboardService clipboardService,
        ITranslationService translationService, ISender sender, IPublisher publisher,
        IEventAggregator eventAggregator)
    {
        _notificationService = notificationService;
        _clipboardService = clipboardService;
        _translationService = translationService;
        _sender = sender;
        _publisher = publisher;
        _eventAggregator = eventAggregator;
        KindChanged = ReactiveCommand.Create<TranslationKind>(OnKindChanged);

        RegisterPrismEvent();
    }

    private void RegisterPrismEvent()
    {
        _eventAggregator.GetEvent<TestEvent>().Subscribe(args =>
        {
            _notificationService?.Show("Prism Event",
                $"模块【SlugifyString】Prism事件处理程序：Args = {args.Args}, Now = {DateTime.Now}");
        });
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
        }
    }

    public Task ExecuteCopyAsync()
    {
        if (!string.IsNullOrWhiteSpace(To))
        {
            _clipboardService?.CopyToAsync(To);
            _notificationService?.Show("成功", "已复制");
        }
        else
        {
            _notificationService?.Show("没有可以复制内容", "请先生成后再复制");
        }

        return Task.CompletedTask;
    }


    private void OnKindChanged(TranslationKind newKind)
    {
        Kind = newKind;
    }
}