using System.Net.Sockets;

namespace CodeWF.Web;

public static class WebApplicationBuilderExtension
{
    public static void WriteParameterTable(this WebApplicationBuilder builder)
    {
        string appVersion = Helper.AppVersion;
        Console.WriteLine($"CodeWF {appVersion} | .NET {Environment.Version}");
        Console.WriteLine("----------------------------------------------------------");

        string strHostName = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
        IPAddress[] ips = ipEntry.AddressList;

        // get all IPv4 addresses
        IPAddress[] ipv4s = ips.Where(p => p.AddressFamily == AddressFamily.InterNetwork).ToArray();

        // get all IPv6 addresses
        IPAddress[] ipv6s = ips.Where(p => p.AddressFamily == AddressFamily.InterNetworkV6).ToArray();

        string? envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        Dictionary<string, string> dic = new Dictionary<string, string>
        {
            { "Path", Environment.CurrentDirectory },
            { "System", Helper.TryGetFullOSVersion() },
            { "User", Environment.UserName },
            { "Host", Environment.MachineName },
            { "IPv4", string.Join(", ", ipv4s.Select(p => p.ToString())) },
            { "IPv6", string.Join(", ", ipv6s.Select(p => p.ToString())) },
            { "URLs", builder.Configuration["Urls"]! },
            { "Database", builder.Configuration.GetConnectionString("DatabaseType")! },
            { "Image storage", builder.Configuration["ImageStorage:Provider"]! },
            { "Authentication", builder.Configuration["Authentication:Provider"]! },
            { "Editor", builder.Configuration["Editor"]! },
            { "Environment", envName ?? "N/A" }
        };

        if (!string.IsNullOrWhiteSpace(envName) && envName.ToLower() == "development")
        {
            dic.Add("Connection String", builder.Configuration.GetConnectionString("CodeWFDatabase")!);
        }

        foreach (KeyValuePair<string, string> item in dic)
        {
            Console.WriteLine($"{item.Key,-20} | {item.Value,-35}");
        }

        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine("https://github.com/dotnet9/CodeWF");
    }
}