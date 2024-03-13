namespace CodeWF.Tools.Core.Services.IPQuery;

public class IPPCOnlineQueryService : IIPQueryService
{
    private readonly HttpClient _httpClient;

    public IPPCOnlineQueryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IPQueryInfo> QueryAsync(string ip, CancellationToken cancellationToken)
    {
        var result = string.Empty;
        try
        {
            var url = $"http://whois.pconline.com.cn/ip.jsp?ip={ip}";
            var str = await _httpClient.GetStringAsync(url, cancellationToken);
            result = str.Trim();
        }
        catch (Exception ex)
        {
            result = ex.Message;
        }

        return new IPQueryInfo("太平洋电脑网", ip, result);
    }
}