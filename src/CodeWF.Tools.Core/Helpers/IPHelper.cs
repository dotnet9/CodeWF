namespace CodeWF.Tools.Core.Helpers;

public static class IPHelper
{
    public static async Task<string?> GetLocalIPAsync()
    {
        var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync("https://ipv4.gdt.qq.com/get_client_ip");
        var str = await response.Content.ReadAsStringAsync();
        return System.Net.IPAddress.TryParse(str, out var ip) ? ip.ToString() : string.Empty;
    }
}