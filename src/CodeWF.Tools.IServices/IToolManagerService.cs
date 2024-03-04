namespace CodeWF.Tools.IServices;

public interface IToolManagerService
{
    void AddTool(ToolType type, string toolName, string toolDescription, string toolRouteKey);
    void RemoveTool(ToolType type, string toolName);

    ObservableCollection<ToolMenuItem> MenuItems { get; set; }
}

public enum ToolType
{
    [Description("开发类")] Developer,
    [Description("网站开发")] Web,
    [Description("测试")] Test,
    [Description("未分类")] Other
}

public class ToolMenuItem
{
    public ToolType ToolType { get; set; }
    public string? Header { get; set; }
    public string? Description { get; set; }
    public int IconIndex { get; set; }
    public bool IsSeparator { get; set; }

    public ToolMenuItem()
    {
        IconIndex = DateTime.Now.Microsecond;
    }

    public ObservableCollection<ToolMenuItem> Children { get; set; } = new();

    public IEnumerable<ToolMenuItem> GetLeaves()
    {
        if (this.Children.Count == 0)
        {
            yield return this;
            yield break;
        }

        foreach (var child in Children)
        {
            var items = child.GetLeaves();
            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}