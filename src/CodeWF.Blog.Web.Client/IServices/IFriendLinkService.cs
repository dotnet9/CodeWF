using CodeWF.Blog.Web.Client.Models.FriendLinks;

namespace CodeWF.Blog.Web.Client.IServices;

public interface IFriendLinkService
{
    Task<List<FriendLink>?> GetFriendLinksAsync();
}