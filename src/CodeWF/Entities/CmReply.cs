namespace CodeWF.Entities;

/// <summary>
/// 回复信息类。
/// </summary>
public class CmReply : EntityBase
{
    /// <summary>
    /// 取得或设置业务类型。
    /// </summary>
    [DisplayName("业务类型")]
    [Required]
    [MaxLength(50)]
    public string BizType { get; set; }

    /// <summary>
    /// 取得或设置业务ID。
    /// </summary>
    [DisplayName("业务ID")]
    [Required]
    [MaxLength(50)]
    public string BizId { get; set; }

    /// <summary>
    /// 取得或设置用户ID。
    /// </summary>
    [DisplayName("用户ID")]
    [Required]
    [MaxLength(50)]
    public string UserId { get; set; }

    /// <summary>
    /// 取得或设置内容。
    /// </summary>
    [DisplayName("内容")]
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// 取得或设置回复时间。
    /// </summary>
    [DisplayName("回复时间")]
    [Required]
    public DateTime ReplyTime { get; set; }

    /// <summary>
    /// 取得或设置喜欢数。
    /// </summary>
    [DisplayName("喜欢数")]
    public int? LikeQty { get; set; }

    /// <summary>
    /// 取得或设置回复数。
    /// </summary>
    [DisplayName("回复数")]
    public int? ReplyQty { get; set; }
}