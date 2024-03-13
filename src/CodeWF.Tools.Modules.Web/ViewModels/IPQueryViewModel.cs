using System.Reactive.Linq;

namespace CodeWF.Tools.Modules.Web.ViewModels;

public class IPQueryViewModel : ViewModelBase
{
    private readonly List<IIPQueryService> _services;
    private readonly INotificationService _notificationService;
    private string? _ipAddress;

    /// <summary>
    /// 输入需要查询的IP
    /// </summary>
    public string? IPAddress
    {
        get => _ipAddress;
        set
        {
            this.RaiseAndSetIfChanged(ref _ipAddress, value);
        }
    }

    public ObservableCollection<IPQueryInfo> IPQueryInfos { get; private set; } = new();

    public IPQueryViewModel(List<IIPQueryService> services, INotificationService _notificationService)
    {
        _services = services;
        this._notificationService = _notificationService;
        this.WhenAnyValue(x => x.IPAddress)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);
    }

    private async void DoSearch(string ip)
    {
        await ExecuteQueryAsync();
    }

    /// <summary>
    /// 查房输入的IP信息
    /// </summary>
    /// <returns></returns>
    public async Task ExecuteQueryAsync()
    {
        async Task QueryIP(IIPQueryService service, CancellationToken token)
        {
            var info = await service.QueryAsync(IPAddress, token);
            IPQueryInfos.Add(info);
        }

        if (string.IsNullOrWhiteSpace(IPAddress))
        {
            return;
        }

        if (!System.Net.IPAddress.TryParse(IPAddress, out _))
        {
            _notificationService?.Show("IP地址格式错误", "请填写正确的IP地址");
            return;
        }

        IPQueryInfos.Clear();
        var tasks = new List<Task>();
        var cancellationTokenSource = new CancellationTokenSource();
        _services.ForEach(service => tasks.Add(QueryIP(service, cancellationTokenSource.Token)));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 查询本机IP地址
    /// </summary>
    /// <returns></returns>
    public async Task ExecuteQueryLocalAsync()
    {
        IPAddress = await IPHelper.GetLocalIPAsync();
        await ExecuteQueryAsync();
    }
}