namespace CodeWF.Repositories;

/// <summary>
/// 前台站点数据依赖接口。
/// </summary>
public interface ISiteRepository
{
    /// <summary>
    /// 异步分页查询内容列表。
    /// </summary>
    /// <param name="criteria">查询条件。</param>
    /// <returns>分页结果。</returns>
    Task<PagingResult<PostListInfo>> QueryPostsAsync(PagingCriteria criteria);

    /// <summary>
    /// 异步获取内容排行榜。
    /// </summary>
    /// <param name="type">内容类型。</param>
    /// <returns>排行榜内容列表。</returns>
    Task<List<PostListInfo>> GetRankPostsAsync(ContentType type);

    /// <summary>
    /// 异步获取内容表单信息。
    /// </summary>
    /// <param name="id">内容ID。</param>
    /// <returns>内容表单信息。</returns>
    Task<PostFormInfo> GetPostAsync(string id);

    /// <summary>
    /// 异步分页查询回复列表。
    /// </summary>
    /// <param name="criteria">查询条件。</param>
    /// <returns>分页结果。</returns>
    Task<PagingResult<ReplyListInfo>> QueryRepliesAsync(PagingCriteria criteria);

    /// <summary>
    /// 异步获取回复表单信息。
    /// </summary>
    /// <param name="id">回复ID。</param>
    /// <returns>回复表单信息。</returns>
    Task<ReplyFormInfo> GetReplyAsync(string id);
}

class SiteRepository : ISiteRepository
{
    private readonly Database db = Database.Create();

    #region Post
    public Task<PagingResult<PostListInfo>> QueryPostsAsync(PagingCriteria criteria)
    {
        var sql = @"select a.*,b.NickName as Author 
from CmPost a 
left join CmUser b on b.Id=a.UserId 
where 1=1";

        var key = criteria.Parameters.GetValue<string>("Key");
        if (!string.IsNullOrWhiteSpace(key))
        {
            sql += " and (a.Title like @Key or a.Content like @Key)";
            criteria.SetQuery("Key", $"%{key}%");
        }

        return db.QueryPageAsync<PostListInfo>(sql, criteria);
    }

    public Task<List<PostListInfo>> GetRankPostsAsync(ContentType type)
    {
        var sql = $"select * from CmPost where Type='{type}' order by RankNo desc limit 5 offset 0";
        return db.QueryListAsync<PostListInfo>(sql, type);
    }

    public Task<PostFormInfo> GetPostAsync(string id)
    {
        var sql = "select * from CmPost where Id=@id";
        return db.QueryAsync<PostFormInfo>(sql, new { id });
    }
    #endregion

    #region Reply
    public Task<PagingResult<ReplyListInfo>> QueryRepliesAsync(PagingCriteria criteria)
    {
        var sql = "select * from CmReply where UserId=@UserId";

        var key = criteria.Parameters.GetValue<string>("Key");
        if (!string.IsNullOrWhiteSpace(key))
        {
            sql += " and Content like @Key";
            criteria.SetQuery("Key", $"%{key}%");
        }

        return db.QueryPageAsync<ReplyListInfo>(sql, criteria);
    }

    public Task<ReplyFormInfo> GetReplyAsync(string id)
    {
        var sql = "select a.Id,a.BizId,a.Content,b.Title from CmReply a,CmPost b where a.BizId=b.Id and a.Id=@id";
        return db.QueryAsync<ReplyFormInfo>(sql, new { id });
    }
    #endregion
}