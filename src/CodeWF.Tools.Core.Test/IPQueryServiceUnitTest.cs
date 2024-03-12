namespace CodeWF.Tools.Core.Test;

[TestClass]
public class IPQueryServiceUnitTest
{
    const string ip = "32.32.5.33";
    private IIPQueryService _taoBaoQueryService;
    private IIPQueryService _baiDuQueryService;
    private IIPQueryService _tencentMapQueryService;
    private IIPQueryService _aMapQueryService;
    private IIPQueryService _pcOnlineQueryService;

    public IPQueryServiceUnitTest()
    {
        var httpClient = new HttpClient();
        _taoBaoQueryService = new IPTaoBaoQueryService(httpClient);
        _baiDuQueryService = new IPBaiDuQueryService(httpClient);
        _tencentMapQueryService = new IPTencentQueryService(httpClient);
        _aMapQueryService = new IPAMapQueryService(httpClient);
        _pcOnlineQueryService = new IPPCOnlineQueryService(httpClient);
    }

    [TestMethod]
    public async Task Test_IPQueryAsync_SUCCESS()
    {
        var cancelTokenSource = new CancellationTokenSource();

        var queryInfoFromTaoBao = await _taoBaoQueryService.QueryAsync(ip, cancelTokenSource.Token);
        var queryInfoFromBaiDu = await _baiDuQueryService.QueryAsync(ip, cancelTokenSource.Token);
        var queryInfoFromTencent = await _tencentMapQueryService.QueryAsync(ip, cancelTokenSource.Token);
        var queryInfoFromAMap = await _aMapQueryService.QueryAsync(ip, cancelTokenSource.Token);
        var queryInfoFromPCOnline = await _pcOnlineQueryService.QueryAsync(ip, cancelTokenSource.Token);

        Assert.IsNotNull(queryInfoFromTaoBao);
        Assert.IsNotNull(queryInfoFromBaiDu);
        Assert.IsNotNull(queryInfoFromTencent);
        Assert.IsNotNull(queryInfoFromAMap);
        Assert.IsNotNull(queryInfoFromPCOnline);
    }
}