namespace CodeWF.Tools.Modules.Web.ViewModels;

public class IPQueryViewModel : ViewModelBase
{
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

    /// <summary>
    /// 查房输入的IP信息
    /// </summary>
    /// <returns></returns>
    public async Task ExecuteQueryAsync()
    {
    }

    /// <summary>
    /// 查询本机IP地址
    /// </summary>
    /// <returns></returns>
    public async Task ExecuteQueryLocalAsync()
    {
    }
}