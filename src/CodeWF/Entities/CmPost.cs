namespace CodeWF.Entities;

/// <summary>
/// 内容信息类。
/// </summary>
public class CmPost : EntityBase
{
    /// <summary>
    /// 取得或设置内容类型。
    /// </summary>
    [DisplayName("内容类型")]
    [Required]
    [MaxLength(50)]
    public string Type { get; set; }

    /// <summary>
    /// 取得或设置用户ID。
    /// </summary>
    [DisplayName("用户ID")]
    [Required]
    [MaxLength(50)]
    public string UserId { get; set; }

    /// <summary>
    /// 取得或设置分类ID。
    /// </summary>
    [DisplayName("分类ID")]
    [MaxLength(50)]
    public string CategoryId { get; set; }

    /// <summary>
    /// 取得或设置标题。
    /// </summary>
    [DisplayName("标题")]
    [Required]
    [MaxLength(250)]
    public string Title { get; set; }

    /// <summary>
    /// 取得或设置内容。
    /// </summary>
    [DisplayName("内容")]
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// 取得或设置摘要。
    /// </summary>
    [DisplayName("摘要")]
    [MaxLength(500)]
    public string Summary { get; set; }

    /// <summary>
    /// 取得或设置标签。
    /// </summary>
    [DisplayName("标签")]
    [MaxLength(200)]
    public string Tags { get; set; }

    /// <summary>
    /// 取得或设置图片。
    /// </summary>
    [DisplayName("图片")]
    [MaxLength(250)]
    public string Image { get; set; }

    /// <summary>
    /// 取得或设置附件。
    /// </summary>
    [DisplayName("附件")]
    [MaxLength(250)]
    public string Files { get; set; }

    /// <summary>
    /// 取得或设置状态。
    /// </summary>
    [DisplayName("状态")]
    [Required]
    [MaxLength(50)]
    public string Status { get; set; }

    /// <summary>
    /// 取得或设置发布时间。
    /// </summary>
    [DisplayName("发布时间")]
    public DateTime? PublishTime { get; set; }

    /// <summary>
    /// 取得或设置浏览数。
    /// </summary>
    [DisplayName("浏览数")]
    public int? ViewQty { get; set; }

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

    /// <summary>
    /// 取得或设置排名。
    /// </summary>
    [DisplayName("排名")]
    public int? RankNo { get; set; }

    /// <summary>
    /// 取得或设置页面编码。
    /// </summary>
    public virtual string Code { get; set; }
}