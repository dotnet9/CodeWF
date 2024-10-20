using System.Xml.Serialization;

namespace CodeWF.Models;

/// <summary>
/// XML文档根节点类。
/// </summary>
[XmlRoot("doc")]
public class DocRootNode
{
    /// <summary>
    /// 取得或设置程序集。
    /// </summary>
    [XmlElement("assembly")]
    public DocAssemblyNode Assembly { get; set; }

    /// <summary>
    /// 取得或设置成员列表。
    /// </summary>
    [XmlArray("members"), XmlArrayItem("member")]
    public List<DocMemberNode> Members { get; set; }
}

/// <summary>
/// XML文档程序集节点类。
/// </summary>
public class DocAssemblyNode
{
    /// <summary>
    /// 取得或设置程序集名称。
    /// </summary>
    [XmlElement("name")]
    public string Name { get; set; }
}

/// <summary>
/// XML文档成员节点类。
/// </summary>
public class DocMemberNode
{
    /// <summary>
    /// 取得或设置成员名称。
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置成员摘要描述。
    /// </summary>
    [XmlElement("summary")]
    public string Summary { get; set; }

    /// <summary>
    /// 取得或设置成员返回参数。
    /// </summary>
    [XmlElement("returns")]
    public string Returns { get; set; }

    /// <summary>
    /// 取得或设置成员参数列表。
    /// </summary>
    [XmlElement("param")]
    public List<DocParamNode> Params { get; set; }

    /// <summary>
    /// 取得或设置成员类型参数列表。
    /// </summary>
    [XmlElement("typeparam")]
    public List<DocParamNode> TypeParams { get; set; }

    /// <summary>
    /// 取得成员是否是类型。
    /// </summary>
    public bool IsType => Name.StartsWith("T:");

    /// <summary>
    /// 取得成员是否是字段。
    /// </summary>
    public bool IsField => Name.StartsWith("F:");

    /// <summary>
    /// 取得成员是否是构造函数。
    /// </summary>
    public bool IsConstructor => Name.StartsWith("M:") && Name.Contains(".#ctor");

    /// <summary>
    /// 取得成员是否是属性。
    /// </summary>
    public bool IsProperty => Name.StartsWith("P:");

    /// <summary>
    /// 取得成员是否是方法。
    /// </summary>
    public bool IsMethod => Name.StartsWith("M:");

    /// <summary>
    /// 判断成员是否属于指定类型的成员。
    /// </summary>
    /// <param name="type">类型。</param>
    /// <returns>是否是类型成员。</returns>
    public bool BelongTo(DocType type) => Name.Substring(2).Replace("`1", "<T>").StartsWith($"{type}.");
}

/// <summary>
/// XML文档参数节点类。
/// </summary>
public class DocParamNode
{
    /// <summary>
    /// 取得或设置参数名称。
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置参数摘要描述。
    /// </summary>
    [XmlText]
    public string Summary { get; set; }
}

/// <summary>
/// 文档程序集类。
/// </summary>
public class DocAssembly
{
    /// <summary>
    /// 取得或设置程序集名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置程序集包含的命名空间列表。
    /// </summary>
    public List<DocNamespace> Namespaces { get; set; } = [];

    /// <summary>
    /// 获取程序集显示字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}

/// <summary>
/// 文档命名空间类。
/// </summary>
public class DocNamespace
{
    /// <summary>
    /// 取得命名空间ID。
    /// </summary>
    public string Id => Name.Replace(".", "");

    /// <summary>
    /// 取得或设置命名空间名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置命名空间包含的类型列表。
    /// </summary>
    public List<DocType> Types { get; set; } = [];

    /// <summary>
    /// 获取命名空间显示字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}

/// <summary>
/// 文档类型类。
/// </summary>
public class DocType
{
    /// <summary>
    /// 构造函数，创建一个文档类型类的实例。
    /// </summary>
    /// <param name="node">XML文档成员节点。</param>
    public DocType(DocMemberNode node)
    {
        var index = node.Name.LastIndexOf('.');
        Namespace = node.Name.Substring(2, index - 2);
        Name = DocHelper.FormatName(node.Name.Substring(index + 1));
        FullName = $"{Namespace}.{Name}";
        Description = DocHelper.GetSummary(node.Summary);
    }

    /// <summary>
    /// 取得类型ID。
    /// </summary>
    public string Id => FullName.Replace(".", "").Replace("<", "").Replace(">", "");

    /// <summary>
    /// 取得或设置类型命名空间。
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// 取得或设置类型名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置类型命名空间加名称的全名。
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 取得或设置类型描述。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 取得或设置类型包含的字段列表。
    /// </summary>
    public List<DocField> Fields { get; set; } = [];

    /// <summary>
    /// 取得或设置类型包含的构造函数列表。
    /// </summary>
    public List<DocMethod> Constructors { get; set; } = [];

    /// <summary>
    /// 取得或设置类型包含的属性列表。
    /// </summary>
    public List<DocProperty> Properties { get; set; } = [];

    /// <summary>
    /// 取得或设置类型包含的公共方法列表。
    /// </summary>
    public List<DocMethod> PublicMethods { get; set; } = [];

    /// <summary>
    /// 取得或设置类型包含的保护方法列表。
    /// </summary>
    public List<DocMethod> ProtectedMethods { get; set; } = [];

    /// <summary>
    /// 获取类型显示的字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => FullName;
}

/// <summary>
/// 文档字段类。
/// </summary>
public class DocField
{
    /// <summary>
    /// 构造函数，创建一个文档字段类的实例。
    /// </summary>
    /// <param name="node">XML文档成员节点。</param>
    public DocField(DocMemberNode node)
    {
        var index = node.Name.LastIndexOf('.');
        Name = DocHelper.FormatName(node.Name.Substring(index + 1));
        Description = DocHelper.GetSummary(node.Summary);
    }

    /// <summary>
    /// 取得或设置字段名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置字段描述。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 获取字段显示的字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}

/// <summary>
/// 文档属性类。
/// </summary>
public class DocProperty
{
    /// <summary>
    /// 构造函数，创建一个文档属性类的实例。
    /// </summary>
    /// <param name="node">XML文档成员节点。</param>
    public DocProperty(DocMemberNode node)
    {
        var index = node.Name.LastIndexOf('.');
        Name = DocHelper.FormatName(node.Name.Substring(index + 1));
        Description = DocHelper.GetSummary(node.Summary);
    }

    /// <summary>
    /// 取得或设置属性名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置属性类型。
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 取得或设置属性描述。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 获取属性显示的字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}

/// <summary>
/// 文档方法类。
/// </summary>
public class DocMethod
{
    /// <summary>
    /// 构造函数，创建一个文档方法类的实例。
    /// </summary>
    /// <param name="node">XML文档成员节点。</param>
    /// <param name="isConstructor">是否是构造函数。</param>
    public DocMethod(DocMemberNode node, bool isConstructor)
    {
        var index = node.Name.LastIndexOf('.');
        Name = DocHelper.FormatName(node.Name.Substring(index + 1));
        FullName = DocHelper.FormatName(node.Name.Substring(2));
        if (!FullName.Contains('('))
            FullName += "()";
        Description = DocHelper.GetSummary(node.Summary);
        Params = node.Params?.Select(n => new DocParam(n)).ToList();
    }

    /// <summary>
    /// 取得或设置方法名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置方法带命名空间的全名。
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 取得或设置方法描述。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 取得或设置方法返回类型。
    /// </summary>
    public string Returns { get; set; }

    /// <summary>
    /// 取得或设置方法包含的参数列表。
    /// </summary>
    public List<DocParam> Params { get; set; } = [];

    /// <summary>
    /// 获取方法显示的字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}

/// <summary>
/// 文档参数类。
/// </summary>
public class DocParam
{
    /// <summary>
    /// 构造函数，创建一个文档参数类的实例。
    /// </summary>
    /// <param name="node">XML文档成员节点。</param>
    public DocParam(DocParamNode node)
    {
        Name = node?.Name;
        Description = DocHelper.GetSummary(node.Summary);
    }

    /// <summary>
    /// 取得或设置参数名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 取得或设置参数描述。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 获取方法显示的字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
}