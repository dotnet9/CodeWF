namespace CodeWF.Tools.IServices;

public interface IToolManagerService
{
    void AddTool(string name, string description, string viewName);
    void AddTool(ToolType group, string name, string description, string viewName);
    void RemoveTool(string name);

    ObservableCollection<ToolMenuItem> MenuItems { get; set; }
}

public enum ToolType
{
    [Description("Home")] Home,
    [Description("开发类")] Developer,
    [Description("网站开发")] Web,
    [Description("测试")] Test,
    [Description("未分类")] Other
}

public class ToolMenuItem
{
    public ToolType Group { get; set; }
    public int Level { get; set; }
    public string? Header { get; set; }
    public string? Description { get; set; }
    public string? ViewName { get; set; }
    public int IconIndex { get; set; } = DateTime.Now.Microsecond;
    public bool IsSeparator { get; set; }

    public ObservableCollection<ToolMenuItem> Children { get; set; } = new();
}