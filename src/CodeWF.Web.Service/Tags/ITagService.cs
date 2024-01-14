namespace CodeWF.Web.Service.Tags;

public interface ITagService
{
    Task<List<TagBrief>> GetTagsAsync(int count);
}