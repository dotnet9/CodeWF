namespace CodeWF;

/// <summary>
/// 内容类型枚举。
/// </summary>
public enum ContentType
{
    /// <summary>
    /// 文章。
    /// </summary>
    Article,
    /// <summary>
    /// 更新日志。
    /// </summary>
    UpdateLog,
    /// <summary>
    /// 文档。
    /// </summary>
    Document,
    /// <summary>
    /// 交流。
    /// </summary>
    Interact
}

/// <summary>
/// 业务类型枚举。
/// </summary>
public enum BizType
{
    /// <summary>
    /// 内容。
    /// </summary>
    Content,
    /// <summary>
    /// 回复。
    /// </summary>
    Reply
}

/// <summary>
/// 用户日志类型枚举。
/// </summary>
public enum UserLogType
{
    /// <summary>
    /// 查看。
    /// </summary>
    View,
    /// <summary>
    /// 点赞。
    /// </summary>
    Like
}