namespace CodeWF.Admin.Services;

public interface IPostService : IService
{
    //Post
    Task<PagingResult<CmPost>> QueryPostsAsync(PagingCriteria criteria);
    Task<CmPost> GetPostAsync(string id);
    Task<Result> DeletePostsAsync(List<CmPost> models);
    Task<Result> SavePostAsync(CmPost model);

    //Reply
    Task<PagingResult<CmReply>> QueryRepliesAsync(PagingCriteria criteria);

    //Log
    Task<PagingResult<CmLog>> QueryLogsAsync(PagingCriteria criteria);
}

class PostService(Context context) : ServiceBase(context), IPostService
{
    //Post
    public Task<PagingResult<CmPost>> QueryPostsAsync(PagingCriteria criteria)
    {
        return Database.QueryPageAsync<CmPost>(criteria);
    }

    public Task<CmPost> GetPostAsync(string id)
    {
        return Database.QueryByIdAsync<CmPost>(id);
    }

    public async Task<Result> DeletePostsAsync(List<CmPost> models)
    {
        if (models == null || models.Count == 0)
            return Result.Error(Language.SelectOneAtLeast);

        return await Database.TransactionAsync(Language.Delete, async db =>
        {
            foreach (var item in models)
            {
                await db.DeleteAsync(item);
            }
        });
    }

    public async Task<Result> SavePostAsync(CmPost model)
    {
        // 交流内容不改变用户ID
        if (model.Type != ContentType.Interact.ToString())
            model.UserId = CurrentUser.UserName;
        var vr = model.Validate(Context);
        if (!vr.IsValid)
            return vr;

        return await Database.TransactionAsync(Language.Save, async db =>
        {
            await db.SaveAsync(model);
        }, model);
    }

    //Reply
    public Task<PagingResult<CmReply>> QueryRepliesAsync(PagingCriteria criteria)
    {
        return Database.QueryPageAsync<CmReply>(criteria);
    }

    //Log
    public Task<PagingResult<CmLog>> QueryLogsAsync(PagingCriteria criteria)
    {
        return Database.QueryPageAsync<CmLog>(criteria);
    }
}