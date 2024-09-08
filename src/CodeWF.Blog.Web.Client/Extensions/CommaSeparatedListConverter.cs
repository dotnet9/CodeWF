using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace CodeWF.Blog.Web.Client.Extensions;

public class CommaSeparatedListConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(List<string>);
    }

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var value = parser.Consume<Scalar>().Value; // 获取当前标量值
        return new List<string>(value.Split(new[] { ',' },
            StringSplitOptions.RemoveEmptyEntries)); // 按逗号分隔并返回 List<string>
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var list = value as List<string>;
        emitter.Emit(new Scalar(string.Join(", ", list))); // 将 List<string> 转换为逗号分隔的字符串
    }
}