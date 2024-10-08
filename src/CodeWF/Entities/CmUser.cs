namespace CodeWF.Entities;

/// <summary>
/// 用户信息类。
/// </summary>
public class CmUser : EntityBase
{
    /// <summary>
    /// 取得或设置账号。
    /// </summary>
    [DisplayName("账号")]
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; }

    /// <summary>
    /// 取得或设置密码。
    /// </summary>
    [DisplayName("密码")]
    [Required]
    [MaxLength(50)]
    public string Password { get; set; }

    /// <summary>
    /// 取得或设置微信ID。
    /// </summary>
    [DisplayName("微信ID")]
    [MaxLength(50)]
    public string OpenId { get; set; }

    /// <summary>
    /// 取得或设置微信ID。
    /// </summary>
    [DisplayName("微信ID")]
    [MaxLength(50)]
    public string UnionId { get; set; }

    /// <summary>
    /// 取得或设置昵称。
    /// </summary>
    [DisplayName("昵称")]
    [MaxLength(50)]
    public string NickName { get; set; }

    /// <summary>
    /// 取得或设置性别。
    /// </summary>
    [DisplayName("性别")]
    [MaxLength(50)]
    public string Sex { get; set; }

    /// <summary>
    /// 取得或设置国家。
    /// </summary>
    [DisplayName("国家")]
    [MaxLength(50)]
    public string Country { get; set; }

    /// <summary>
    /// 取得或设置省份。
    /// </summary>
    [DisplayName("省份")]
    [MaxLength(50)]
    public string Province { get; set; }

    /// <summary>
    /// 取得或设置城市。
    /// </summary>
    [DisplayName("城市")]
    [MaxLength(50)]
    public string City { get; set; }

    /// <summary>
    /// 取得或设置头像。
    /// </summary>
    [DisplayName("头像")]
    [MaxLength(250)]
    public string AvatarUrl { get; set; }

    /// <summary>
    /// 取得或设置职业。
    /// </summary>
    [DisplayName("职业")]
    [MaxLength(50)]
    public string Metier { get; set; }

    /// <summary>
    /// 取得或设置状态。
    /// </summary>
    [DisplayName("状态")]
    [Required]
    [MaxLength(50)]
    public string Status { get; set; }

    /// <summary>
    /// 取得或设置积分。
    /// </summary>
    [DisplayName("积分")]
    public int? Integral { get; set; }

    /// <summary>
    /// 取得或设置内容数。
    /// </summary>
    [DisplayName("内容数")]
    public int? ContentQty { get; set; }

    /// <summary>
    /// 取得或设置回复数。
    /// </summary>
    [DisplayName("回复数")]
    public int? ReplyQty { get; set; }
}