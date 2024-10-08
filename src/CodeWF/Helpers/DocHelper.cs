namespace CodeWF.Helpers;

class DocHelper
{
    private static readonly List<DocRootNode> Docs = [];
    internal static List<DocAssembly> Assemblies = [];

    internal static void Initialize()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "api");
        if (!Directory.Exists(path))
            return;

        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            var xml = File.ReadAllText(file);
            var node = Utils.FromXml<DocRootNode>(xml);
            if (node != null)
            {
                Docs.Add(node);

                var doc = new DocAssembly();
                doc.Name = node.Assembly.Name;
                doc.Namespaces = GetNamespaces(node);
                Assemblies.Add(doc);
            }
        }

        Assemblies = [.. Assemblies.OrderBy(m => m.Name)];
    }

    internal static string GetSummary(string summary) => summary?.Trim('\n').Trim();

    internal static string FormatName(string name)
    {
        return name.Replace("``2", "<T,T1>")
                   .Replace("{``0,``1}", "<T,T1>")
                   .Replace("<`0,", "<T,")
                   .Replace("``0", "T")
                   .Replace("``1", "<T>")
                   .Replace("`1", "<T>")
                   .Replace("{", "<")
                   .Replace("}", ">")
                   .Replace(".#ctor", "")
                   .Replace("@", "")
                   .Replace("System.Collections.Generic.", "")
                   .Replace("System.Linq.Expressions.", "")
                   .Replace("System.Reflection.", "")
                   .Replace("System.Threading.Tasks.", "")
                   .Replace("System.Collections.", "")
                   .Replace("System.IO.", "")
                   .Replace("System.Data.", "")
                   .Replace("System.", "")
                   .Replace("Microsoft.JSInterop.", "")
                   .Replace("Microsoft.Extensions.DependencyInjection.", "")
                   .Replace("Microsoft.Extensions.Configuration.", "")
                   .Replace("Microsoft.AspNetCore.Builder.", "")
                   .Replace("Microsoft.AspNetCore.Http.", "")
                   .Replace("Microsoft.AspNetCore.Components.Rendering.", "")
                   .Replace("Microsoft.AspNetCore.Components.Forms.", "")
                   .Replace("Microsoft.AspNetCore.Components.Web.", "")
                   .Replace("Microsoft.AspNetCore.Components.", "");
    }

    private static List<DocNamespace> GetNamespaces(DocRootNode node)
    {
        var members = node.Members.Where(m => m.IsType).ToList();
        var namespaces = new List<DocNamespace>();
        foreach (var item in members)
        {
            var type = new DocType(item);
            var namespaze = namespaces.FirstOrDefault(m => m.Name == type.Namespace);
            if (namespaze == null)
            {
                namespaze = new DocNamespace { Name = type.Namespace };
                namespaces.Add(namespaze);
            }
            type.Fields = node.Members.Where(m => m.IsField && m.BelongTo(type)).Select(m => new DocField(m)).ToList();
            type.Constructors = node.Members.Where(m => m.IsConstructor && m.BelongTo(type)).Select(m => new DocMethod(m, true)).ToList();
            type.Properties = node.Members.Where(m => m.IsProperty && m.BelongTo(type)).Select(m => new DocProperty(m)).ToList();
            type.PublicMethods = node.Members.Where(m => m.IsMethod && !m.IsConstructor && m.BelongTo(type)).Select(m => new DocMethod(m, false)).ToList();
            namespaze.Types.Add(type);
        }
        foreach (var item in namespaces)
        {
            item.Types = [.. item.Types.OrderBy(m => m.Name)];
        }
        namespaces = [.. namespaces.OrderBy(m => m.Name)];
        return namespaces;
    }
}