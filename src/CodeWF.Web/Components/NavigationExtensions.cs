using Microsoft.AspNetCore.Components;

namespace CodeWF.Web.Components;

public static class NavigationExtensions
{
    /// <summary>取当前 URI 的路径部分（不含 query）。</summary>
    public static string UriPath(this NavigationManager navigation)
    {
        var uri = navigation.ToAbsoluteUri(navigation.Uri);
        return uri.AbsolutePath;
    }
}
