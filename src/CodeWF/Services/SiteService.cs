namespace CodeWF.Services;

/// <summary>
/// 内容站点服务接口。
/// </summary>
public interface ISiteService : IService
{
    #region Category

    /// <summary>
    /// 异步获取内容类别列表。
    /// </summary>
    /// <param name="type">内容类型。</param>
    /// <returns>内容类别列表。</returns>
    Task<List<CmCategory>> GetCategoriesAsync(ContentType type);

    #endregion

    #region Post

    /// <summary>
    /// 异步分页查询内容列表。
    /// </summary>
    /// <param name="criteria">查询条件。</param>
    /// <returns>分页结果。</returns>
    Task<PagingResult<PostListInfo>> QueryPostsAsync(PagingCriteria criteria);

    /// <summary>
    /// 异步获取内容排名列表。
    /// </summary>
    /// <param name="type">内容类型。</param>
    /// <returns>内容列表。</returns>
    Task<List<PostListInfo>> GetRankPostsAsync(ContentType type);

    /// <summary>
    /// 异步获取内容明细信息。
    /// </summary>
    /// <param name="type">内容类型。</param>
    /// <param name="code">内容代码或ID。</param>
    /// <returns>内容明细信息。</returns>
    Task<PostDetailInfo> GetPostAsync(ContentType type, string code);

    /// <summary>
    /// 异步获取内容表单信息。
    /// </summary>
    /// <param name="id">内容ID。</param>
    /// <returns>内容表单信息。</returns>
    Task<PostFormInfo> GetPostAsync(string id);

    /// <summary>
    /// 异步保存发布的内容信息。
    /// </summary>
    /// <param name="info">内容表单信息。</param>
    /// <returns>保存结果。</returns>
    Task<Result> SavePostAsync(PostFormInfo info);

    #endregion

    #region Reply

    /// <summary>
    /// 异步分页查询回复列表。
    /// </summary>
    /// <param name="criteria">查询条件。</param>
    /// <returns>分页结果。</returns>
    Task<PagingResult<ReplyListInfo>> QueryRepliesAsync(PagingCriteria criteria);

    /// <summary>
    /// 异步获取回复内容表单信息。
    /// </summary>
    /// <param name="id">回复内容ID。</param>
    /// <returns>回复内容表单信息。</returns>
    Task<ReplyFormInfo> GetReplyAsync(string id);

    /// <summary>
    /// 异步保存回复内容信息。
    /// </summary>
    /// <param name="info">回复表单信息。</param>
    /// <returns>保存结果。</returns>
    Task<Result> SaveReplyAsync(ReplyFormInfo info);

    #endregion

    #region User

    /// <summary>
    /// 异步获取系统用户信息。
    /// </summary>
    /// <param name="id">用户ID。</param>
    /// <returns>系统用户信息。</returns>
    Task<CmUser> GetUserAsync(string id);

    /// <summary>
    /// 异步保存用户信息。
    /// </summary>
    /// <param name="info">用户表单信息。</param>
    /// <returns>保存结果。</returns>
    Task<Result> SaveUserAsync(UserFormInfo info);

    /// <summary>
    /// 异步保存用户密码。
    /// </summary>
    /// <param name="info">用户密码信息。</param>
    /// <returns>保存结果。</returns>
    Task<Result> SaveUserPwdAsync(PwdFormInfo info);

    #endregion
}

class SiteService(Context context) : ServiceBase(context), ISiteService
{
    private readonly ISiteRepository Repository = Config.GetScopeService<ISiteRepository>();

    #region Category

    public Task<List<CmCategory>> GetCategoriesAsync(ContentType type)
    {
        return Database.QueryListAsync<CmCategory>(d => d.Type == type.ToString());
    }

    #endregion

    #region Post

    public Task<PagingResult<PostListInfo>> QueryPostsAsync(PagingCriteria criteria)
    {
        return Repository.QueryPostsAsync(criteria);
    }

    public Task<List<PostListInfo>> GetRankPostsAsync(ContentType type)
    {
        return Repository.GetRankPostsAsync(type);
    }

    public async Task<PostDetailInfo> GetPostAsync(ContentType type, string code)
    {
        var database = Database;
        await database.OpenAsync();
        var post = type == ContentType.Interact
            ? await database.QueryByIdAsync<CmPost>(code)
            : await GetPostByCodeAsync(database, type, code);
        if (post != null)
        {
            //添加查看记录，同IP记录一次
            var ip = Context.IPAddress;
            var userName = Context.CurrentUser?.UserName ?? "Anonymous";
            await database.AddViewLogAsync(post, ip, userName);
        }

        var info = new PostDetailInfo
        {
            Id = post?.Id,
            Title = post?.Title,
            Content = post?.Content,
            Author = post?.CreateBy,
            Tags = post?.Tags,
            ViewQty = post?.ViewQty,
            LikeQty = post?.LikeQty,
            ReplyQty = post?.ReplyQty
        };
        if (post != null)
        {
            if (!string.IsNullOrWhiteSpace(post.UserId))
            {
                var user = await database.QueryByIdAsync<CmUser>(post.UserId);
                info.Author = user?.NickName;
            }

            if (type == ContentType.Interact)
                info.Replies = await GetReplyListsAsync(database, post.Id);
        }

        await database.CloseAsync();
        return info;
    }

    public Task<PostFormInfo> GetPostAsync(string id)
    {
        return Repository.GetPostAsync(id);
    }

    public async Task<Result> SavePostAsync(PostFormInfo info)
    {
        var database = Database;
        var categories = await database.QueryListAsync<CmCategory>(d => d.Type == ContentType.Interact.ToString());
        var model = await database.QueryByIdAsync<CmPost>(info.Id);
        if (model == null)
        {
            model = new CmPost
            {
                Type = ContentType.Interact.ToString(),
                UserId = CurrentUser?.Id,
                CategoryId = AIHelper.GetPostCategory(categories, info),
                Tags = AIHelper.GetPostTags(info),
                Status = PostStatus.Published,
                PublishTime = DateTime.Now
            };
        }

        model.Title = info.Title;
        model.Content = info.Content;
        var vr = model.Validate(Context);
        if (!vr.IsValid)
            return vr;

        return await database.TransactionAsync(Language.Save, async db =>
        {
            await db.SaveAsync(model);
            await db.AddUserCountAsync(BizType.Content, CurrentUser.Id);
        }, model);
    }

    private static async Task<CmPost> GetPostByCodeAsync(Database db, ContentType type, string code)
    {
        // 查询文章
        if (type == ContentType.Article)
            return await db.QueryAsync<CmPost>(d => d.Type == type.ToString() && d.CategoryId == code);

        // 查询文档
        var categories = await db.QueryListAsync<CmCategory>(d => d.Type == type.ToString());
        if (string.IsNullOrWhiteSpace(code))
        {
            var first = categories?.Where(c => c.ParentId == "0").OrderBy(c => c.Sort).FirstOrDefault();
            if (type == ContentType.UpdateLog)
            {
                first = categories?.Where(c => c.ParentId == "0").OrderBy(c => c.Sort).LastOrDefault();
                code = first?.Code;
            }
            else
            {
                var firstPage = categories?.Where(c => c.ParentId == first?.Id).OrderBy(c => c.Sort).FirstOrDefault();
                code = firstPage?.Code;
            }
        }

        var category = categories.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (category == null)
            return null;

        return await db.QueryByIdAsync<CmPost>(category.Id);
    }

    #endregion

    #region Reply

    public Task<PagingResult<ReplyListInfo>> QueryRepliesAsync(PagingCriteria criteria)
    {
        return Repository.QueryRepliesAsync(criteria);
    }

    public Task<ReplyFormInfo> GetReplyAsync(string id)
    {
        return Repository.GetReplyAsync(id);
    }

    public async Task<Result> SaveReplyAsync(ReplyFormInfo info)
    {
        var database = Database;
        var post = await database.QueryByIdAsync<CmPost>(info.BizId);
        if (post == null)
            return Result.Error("问题不存在！");

        CmReply model = null;
        if (!string.IsNullOrWhiteSpace(info.Id))
            model = await database.QueryByIdAsync<CmReply>(info.Id);
        if (model == null)
        {
            model = new CmReply
            {
                BizType = post.Type,
                BizId = info.BizId,
                UserId = CurrentUser?.Id,
                ReplyTime = DateTime.Now
            };
        }

        model.Content = info.Content;
        var vr = model.Validate(Context);
        if (!vr.IsValid)
            return vr;

        return await database.TransactionAsync(Language.Save, async db =>
        {
            post.ReplyQty = (post.ReplyQty ?? 0) + 1;
            post.CalculateRank();
            await db.SaveAsync(post);
            await db.SaveAsync(model);
            await db.AddUserCountAsync(BizType.Reply, CurrentUser.Id);
        }, model);
    }

    private static async Task<List<ReplyListInfo>> GetReplyListsAsync(Database db, string postId)
    {
        var replies = await db.QueryListAsync<CmReply>(d => d.BizId == postId);
        if (replies == null || replies.Count == 0)
            return null;

        var infos = new List<ReplyListInfo>();
        replies = [.. replies.OrderByDescending(c => c.ReplyTime)];
        foreach (var item in replies)
        {
            var user = await db.QueryByIdAsync<CmUser>(item.UserId);
            var info = new ReplyListInfo
            {
                Id = item.Id,
                BizId = item.BizId,
                Author = new UserInfo
                {
                    Name = user?.NickName,
                    AvatarUrl = user?.AvatarUrl
                },
                Content = item.Content,
                ReplyTime = item.ReplyTime
            };
            infos.Add(info);
        }

        return infos;
    }

    #endregion

    #region User

    public Task<CmUser> GetUserAsync(string id)
    {
        return Database.QueryByIdAsync<CmUser>(id);
    }

    public async Task<Result> SaveUserAsync(UserFormInfo info)
    {
        var database = Database;
        var model = await database.QueryByIdAsync<CmUser>(Context.CurrentUser.Id);
        if (model == null)
            return Result.Error("用户不存在！");

        model.UserName = info.UserName;
        model.NickName = info.NickName;
        model.Sex = info.Sex;
        model.Metier = info.Metier;
        var count = await database.CountAsync<CmUser>(d => d.Id != model.Id && d.UserName == model.UserName);
        if (count > 0)
            return Result.Error("用户名已存在！");

        return await database.TransactionAsync(Language.Save, async db => { await db.SaveAsync(model); }, model);
    }

    public async Task<Result> SaveUserPwdAsync(PwdFormInfo info)
    {
        var database = Database;
        var model = await database.QueryByIdAsync<CmUser>(Context.CurrentUser.Id);
        if (model == null)
            return Result.Error("用户不存在！");

        if (info.NewPwd != info.NewPwd1)
            return Result.Error("两次密码不一致！");

        return await database.TransactionAsync(Language.Save, async db =>
        {
            model.Password = Utils.ToMd5(info.NewPwd);
            await db.SaveAsync(model);
        }, model);
    }

    #endregion
}