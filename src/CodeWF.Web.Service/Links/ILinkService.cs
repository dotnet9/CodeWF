namespace CodeWF.Web.Service.Links;

public interface ILinkService
{
    Task<List<LinkBrief>?> GetListAsync();
}