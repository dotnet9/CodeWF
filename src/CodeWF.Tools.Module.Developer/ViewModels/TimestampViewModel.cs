namespace CodeWF.Tools.Module.Developer.ViewModels;

public class TimestampViewModel : ViewModelBase
{
    private long _currentTimestamp;

    public long CurrentTimestamp
    {
        get => _currentTimestamp;
        set => SetProperty(ref _currentTimestamp, value);
    }

    private CancellationToken _cancellationCalcTimestampToken;

    public async Task RunCalcTimestamp()
    {
        StopCalcTimestamp();
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        });
    }
    public async Task StopCalcTimestamp()
    {
        
    }
}