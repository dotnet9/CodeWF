using System.ComponentModel.DataAnnotations.Schema;

namespace CodeWF.Models;

/// <summary>
/// 内容列表项目信息类。
/// </summary>
[Table(nameof(CmPost))]
public class PostListInfo
{
    /// <summary>
    /// 取得或设置内容ID。
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 取得或设置内容标题。
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 取得或设置内容摘要。
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 取得或设置内容作者。
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// 取得或设置内容标签。
    /// </summary>
    public string Tags { get; set; }

    /// <summary>
    /// 取得或设置内容列表展示图片。
    /// </summary>
    public string Image { get; set; }

    /// <summary>
    /// 取得或设置发布时间。
    /// </summary>
    public DateTime? PublishTime { get; set; }

    /// <summary>
    /// 取得或设置内容浏览次数。
    /// </summary>
    public int? ViewQty { get; set; }

    /// <summary>
    /// 取得或设置内容点赞次数。
    /// </summary>
    public int? LikeQty { get; set; }

    /// <summary>
    /// 取得或设置回复数。
    /// </summary>
    public int? ReplyQty { get; set; }
}

/// <summary>
/// 内容明细信息类。
/// </summary>
public class PostDetailInfo : PostListInfo
{
    /// <summary>
    /// 取得或设置内容的详细内容。
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 取得或设置回复信息列表。
    /// </summary>
    public List<ReplyListInfo> Replies { get; set; }
}

/// <summary>
/// 内容发布表单信息类。
/// </summary>
public class PostFormInfo
{
    /// <summary>
    /// 取得或设置内容ID。
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 取得或设置内容标题。
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 取得或设置内容的详细内容。
    /// </summary>
    public string Content { get; set; }
}

/// <summary>
/// 内容回复列表信息类。
/// </summary>
public class ReplyListInfo
{
    /// <summary>
    /// 取得或设置回复ID。
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 取得或设置内容ID。
    /// </summary>
    public string BizId { get; set; }

    /// <summary>
    /// 取得或设置回复作者。
    /// </summary>
    public UserInfo Author { get; set; }

    /// <summary>
    /// 取得或设置回复的详细内容。
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 取得或设置回复时间。
    /// </summary>
    public DateTime ReplyTime { get; set; }

    /// <summary>
    /// 取得或设置内容点赞次数。
    /// </summary>
    public int? LikeQty { get; set; }
}

/// <summary>
/// 内容回复表单信息类。
/// </summary>
public class ReplyFormInfo
{
    /// <summary>
    /// 取得或设置回复ID。
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 取得或设置内容ID。
    /// </summary>
    public string BizId { get; set; }

    /// <summary>
    /// 取得或设置内容标题。
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 取得或设置回复的详细内容。
    /// </summary>
    public string Content { get; set; }
}