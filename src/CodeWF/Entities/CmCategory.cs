namespace CodeWF.Entities;

/// <summary>
/// 分类信息类。
/// </summary>
public class CmCategory : EntityBase
{
    /// <summary>
    /// 取得或设置内容类型（交流、文档）。
    /// </summary>
    [DisplayName("内容类型")]
    [Required]
    [MaxLength(50)]
    public string Type { get; set; }

    /// <summary>
    /// 取得或设置上级分类。
    /// </summary>
    [DisplayName("上级分类")]
    [Required]
    [MaxLength(50)]
    public string ParentId { get; set; }

    /// <summary>
    /// 取得或设置代码。
    /// </summary>
    [DisplayName("代码")]
    [Required]
    [MaxLength(50)]
    public string Code { get; set; }

    /// <summary>
    /// 取得或设置名称。
    /// </summary>
    [DisplayName("名称")]
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置图标。
    /// </summary>
    [DisplayName("图标")]
    [MaxLength(50)]
    public string Icon { get; set; }

    /// <summary>
    /// 取得或设置顺序。
    /// </summary>
    [DisplayName("顺序")]
    public int? Sort { get; set; }

    /// <summary>
    /// 取得或设置启用。
    /// </summary>
    [DisplayName("启用")]
    [Required]
    public bool Enabled { get; set; }

    /// <summary>
    /// 取得或设置备注。
    /// </summary>
    [DisplayName("备注")]
    public string Note { get; set; }

    /// <summary>
    /// 取得或设置上级类别对象。
    /// </summary>
    public virtual CmCategory Parent { get; set; }

    /// <summary>
    /// 取得或设置下级类别列表。
    /// </summary>
    public virtual List<CmCategory> Children { get; set; } = [];

    /// <summary>
    /// 取得类别全部级别的全名。
    /// </summary>
    public virtual string FullName
    {
        get
        {
            if (Parent != null)
                return Parent.FullName + "/" + Name;

            return Name;
        }
    }

    /// <summary>
    /// 取得更新日志URL。
    /// </summary>
    public virtual string LogUrl => Url.GetLogUrl(Code);

    /// <summary>
    /// 取得文档板块URL。
    /// </summary>
    public virtual string DocUrl => Url.GetDocUrl(Code);

    /// <summary>
    /// 取得API板块URL。
    /// </summary>
    public virtual string ApiUrl => Url.GetApiUrl(Code);

    /// <summary>
    /// 取得交流板块URL。
    /// </summary>
    public virtual string BbsUrl => Url.GetBbsUrl(Code);

    internal CmCategory AddChild(CmCategory child)
    {
        child.Parent = this;
        Children ??= [];
        Children.Add(child);
        return child;
    }
}