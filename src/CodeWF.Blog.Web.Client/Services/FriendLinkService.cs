using CodeWF.Blog.Web.Client.Helpers;
using CodeWF.Blog.Web.Client.IServices;
using CodeWF.Blog.Web.Client.Models.FriendLinks;
using CodeWF.Blog.Web.Client.Options;
using Microsoft.Extensions.Options;

namespace CodeWF.Blog.Web.Client.Services;

public class FriendLinkService(IOptions<SiteOption> site) : IFriendLinkService
{
    private static List<FriendLink>? _links;

    public async Task<List<FriendLink>?> GetFriendLinksAsync()
    {
        _links ??= await AssetsHelper.ReadFriendLinks(site.Value.LocalAssetsDir);
        return _links;
    }
}