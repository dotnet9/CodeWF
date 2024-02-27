namespace CodeWF.MetaWeblog;

public class XmlRpcMethodAttribute(string methodName) : Attribute
{
    public string MethodName { get; set; } = methodName;
}