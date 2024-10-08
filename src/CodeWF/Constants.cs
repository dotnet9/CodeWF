namespace CodeWF;

/// <summary>
/// 内容状态。
/// </summary>
public class PostStatus
{
    /// <summary>
    /// 已发布。
    /// </summary>
    public const string Published = "已发布";

    /// <summary>
    /// 审核中。
    /// </summary>
    public const string Verifing = "审核中";

    /// <summary>
    /// 未通过。
    /// </summary>
    public const string Failed = "未通过";
}

class Url
{
    public const string BbsUrl = "./bbs";
    public const string PostFormUrl = "./bbs/postform";
    public const string UserCreatorUrl = "./creator";
    public const string UserPostsUrl = "./creator/posts";
    public const string UserRepliesUrl = "./creator/replies";
    public const string UserViewUrl = "./creator/views";
    public const string UserLikeUrl = "./creator/likes";

    public static string GetPostForm(string id) => $"./creator/post/{id}";
    public static string GetReplyForm(string id) => $"./creator/reply/{id}";

    public static string GetLogUrl(string code) => $"./log/{code.ToLower()}";
    public static string GetDocUrl(string code) => $"./doc/{code.ToLower()}";
    public static string GetApiUrl(string code) => $"./api/{code.ToLower()}";
    public static string GetBbsUrl(string code) => $"./bbs/{code.ToLower()}";
    public static string GetBbsPostUrl(string id) => $"./bbs/post/{id}";
    public static string GetBbsTagUrl(string tag) => $"./bbs/tag/{tag}";
}