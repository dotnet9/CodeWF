namespace CodeWF.EntityFramework;

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
        //$"select * from CmPost where Type='{type}' order by RankNo desc limit 5 offset 0";
        var ctx = db.DbContext as DataContext;
        return ctx.Set<CmPost>()
                  .Where(d => d.Type == type.ToString())
                  .OrderByDescending(d => d.RankNo)
                  .Take(5)
                  .Select(d => new PostListInfo { Id = d.Id, Title = d.Title })
                  .ToListAsync();
    }

    public Task<PostFormInfo> GetPostAsync(string id)
    {
        var ctx = db.DbContext as DataContext;
        return ctx.Set<CmPost>()
                  .Where(d => d.Id == id)
                  .Select(d => new PostFormInfo
                  {
                      Id = d.Id,
                      Title = d.Title,
                      Content = d.Content
                  })
                  .FirstOrDefaultAsync();
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
        //"select a.Id,a.BizId,a.Content,b.Title from CmReply a,CmPost b where a.BizId=b.Id and a.Id=@id";
        var ctx = db.DbContext as DataContext;
        return ctx.Set<CmReply>()
                  .Join(ctx.Set<CmPost>(), r => r.BizId, p => p.Id, (r, p) => new { r, p })
                  .Where(d => d.r.Id == id)
                  .Select(d => new ReplyFormInfo
                  {
                      Id = d.r.Id,
                      BizId = d.r.BizId,
                      Title = d.p.Title,
                      Content = d.r.Content
                  })
                  .FirstOrDefaultAsync();
    }
    #endregion
}