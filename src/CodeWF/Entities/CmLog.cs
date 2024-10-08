namespace CodeWF.Entities;

/// <summary>
/// 用户操作日志类。
/// </summary>
public class CmLog : EntityBase
{
    /// <summary>
    /// 取得或设置业务类型。
    /// </summary>
    [DisplayName("业务类型")]
    [Required]
    [MaxLength(50)]
    public string BizType { get; set; }

    /// <summary>
    /// 取得或设置日志类型。
    /// </summary>
    [DisplayName("日志类型")]
    [Required]
    [MaxLength(50)]
    public string LogType { get; set; }

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
    /// 取得或设置用户IP。
    /// </summary>
    [DisplayName("用户IP")]
    [MaxLength(50)]
    public string UserIP { get; set; }
}