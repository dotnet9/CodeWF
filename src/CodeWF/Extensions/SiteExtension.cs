namespace CodeWF.Extensions;

static class SiteExtension
{
    internal static async Task AddUserCountAsync(this Database db, BizType type, string userId)
    {
        var user = await db.QueryByIdAsync<CmUser>(userId);
        if (user == null)
            return;

        switch (type)
        {
            case BizType.Content:
                user.ContentQty = (user.ContentQty ?? 0) + 1;
                break;
            case BizType.Reply:
                user.ReplyQty = (user.ReplyQty ?? 0) + 1;
                break;
        }
        await db.SaveAsync(user);
    }

    internal static async Task AddViewLogAsync(this Database db, CmPost post, string ip, string userName)
    {
        var log = await db.Query<CmLog>()
                          .Where(d => d.BizId == post.Id && d.UserIP == ip)
                          .OrderByDescending(d => d.CreateTime)
                          .FirstAsync();
        if (log != null && log.CreateTime.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
            return;

        var log1 = new CmLog
        {
            BizType = post.Type,
            LogType = UserLogType.View.ToString(),
            BizId = post.Id,
            UserId = userName,
            UserIP = ip
        };
        await db.SaveAsync(log1);
        post.ViewQty = (post.ViewQty ?? 0) + 1;
        post.CalculateRank();
        await db.SaveAsync(post);
    }
}