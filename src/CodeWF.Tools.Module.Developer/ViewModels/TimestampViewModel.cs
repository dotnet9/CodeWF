using Avalonia.Media;
using CodeWF.Utils;
using ReactiveUI;

namespace CodeWF.Tools.Module.Developer.ViewModels;

public class TimestampViewModel : ViewModelBase
{
    #region 当前时间

    private bool _isCalcTimestamp;

    public bool IsCalcTimestamp
    {
        get => _isCalcTimestamp;
        set => this.RaiseAndSetIfChanged(ref _isCalcTimestamp, value);
    }

    private string? _calcButtonContent;

    public string? CalcButtonContent
    {
        get => _calcButtonContent;
        set => this.RaiseAndSetIfChanged(ref _calcButtonContent, value);
    }

    private IImmutableSolidColorBrush? _calcButtonForeground;

    public IImmutableSolidColorBrush? CalcButtonForeground
    {
        get => _calcButtonForeground;
        set => this.RaiseAndSetIfChanged(ref _calcButtonForeground, value);
    }

    private TimestampType _currentTimestampType;

    public TimestampType CurrentTimestampType
    {
        get => _currentTimestampType;
        set => this.RaiseAndSetIfChanged(ref _currentTimestampType, value);
    }

    private DateTimeOffset _currentUtcTime;

    public DateTimeOffset CurrentUtcTime
    {
        get => _currentUtcTime;
        set => this.RaiseAndSetIfChanged(ref _currentUtcTime, value);
    }

    private long _currentTimestamp;

    public long CurrentTimestamp
    {
        get => _currentTimestamp;
        set => this.RaiseAndSetIfChanged(ref _currentTimestamp, value);
    }

    #endregion

    public IEnumerable<string> TimestampTypeDescriptions { get; } =
        Enum.GetValues<TimestampType>().Select(t => t.Description());

    #region 时间戳转时间

    private long _timestampFrom;

    public long TimestampFrom
    {
        get => _timestampFrom;
        set => this.RaiseAndSetIfChanged(ref _timestampFrom, value);
    }


    private int _timestampToTimeKindIndex;

    public int TimestampToTimeKindIndex
    {
        get => _timestampToTimeKindIndex;
        set => this.RaiseAndSetIfChanged(ref _timestampToTimeKindIndex, value);
    }

    private DateTimeOffset _timeTo;

    public DateTimeOffset TimeTo
    {
        get => _timeTo;
        set => this.RaiseAndSetIfChanged(ref _timeTo, value);
    }

    #endregion

    #region 时间转时间戳

    private DateTimeOffset _timeFrom;

    public DateTimeOffset TimeFrom
    {
        get => _timeFrom;
        set => this.RaiseAndSetIfChanged(ref _timeFrom, value);
    }

    private int _timeToTimestampKindIndex;

    public int TimeToTimestampKindIndex
    {
        get => _timeToTimestampKindIndex;
        set => this.RaiseAndSetIfChanged(ref _timeToTimestampKindIndex, value);
    }

    private long _timestampTo;

    public long TimestampTo
    {
        get => _timestampTo;
        set => this.RaiseAndSetIfChanged(ref _timestampTo, value);
    }

    #endregion

    public TimestampViewModel()
    {
        _ = RunCalcTimestamp();
        this.WhenAnyValue(x => x.IsCalcTimestamp).Subscribe(newValue => CalcButtonContent = newValue ? "停止" : "开始");
        this.WhenAnyValue(x => x.IsCalcTimestamp)
            .Subscribe(newValue => CalcButtonForeground = newValue ? Brushes.Red : Brushes.Green);
        TimestampFrom = TimestampHelper.GetCurrentTimestamp();
        TimeFrom = DateTimeOffset.UtcNow;
        ExecuteTimestampToTimeCommand();
        ExecuteTimeToTimestampCommand();
    }

    private CancellationTokenSource? _cancellationCalcTimestampTokenSource;

    public async Task ExecuteRunCalcTimestampCommand()
    {
        if (_isCalcTimestamp)
        {
            await StopCalcTimestampAsync();
        }
        else
        {
            await RunCalcTimestamp();
        }
    }

    public void ExecuteTimestampToTimeCommand()
    {
        var kind = (TimestampType)Enum.Parse(typeof(TimestampType), TimestampToTimeKindIndex.ToString());
        TimeTo = TimestampHelper.GetTime(TimestampFrom, kind);
    }

    public void ExecuteTimeToTimestampCommand()
    {
        var kind = (TimestampType)Enum.Parse(typeof(TimestampType), TimeToTimestampKindIndex.ToString());
        TimestampTo = TimestampHelper.GetTimestamp(TimeFrom, kind);
    }

    private async Task RunCalcTimestamp()
    {
        await StopCalcTimestampAsync();
        _cancellationCalcTimestampTokenSource = new CancellationTokenSource();
        IsCalcTimestamp = true;
        await Task.Run(async () =>
        {
            while (_cancellationCalcTimestampTokenSource.IsCancellationRequested == false)
            {
                CurrentUtcTime = DateTimeOffset.UtcNow;
                CurrentTimestamp = TimestampHelper.GetTimestamp(CurrentUtcTime, CurrentTimestampType);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }, _cancellationCalcTimestampTokenSource.Token);
    }

    private async Task StopCalcTimestampAsync()
    {
        if (_cancellationCalcTimestampTokenSource != null)
        {
            await _cancellationCalcTimestampTokenSource.CancelAsync();
        }

        IsCalcTimestamp = false;
    }
}