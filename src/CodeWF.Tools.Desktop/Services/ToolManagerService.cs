namespace CodeWF.Tools.Desktop.Services;

internal class ToolManagerService : IToolManagerService
{
    public void AddTool(ToolType type, string toolName, string toolDescription, string toolRouteKey)
    {
        RemoveTool(type, toolName);
        var toolGroup = MenuItems.FirstOrDefault(item => item.ToolType == type);
        if (toolGroup == null)
        {
            var desc = type.Description();
            toolGroup = new ToolMenuItem() { Header = desc, Description = desc };
            MenuItems.Add(toolGroup);
        }

        toolGroup.Children.Add(new ToolMenuItem { Header = toolName, Description = toolDescription });
    }

    public void RemoveTool(ToolType type, string toolName)
    {
        var toolGroup = MenuItems.FirstOrDefault(item => item.ToolType == type);
        if (toolGroup == null)
        {
            return;
        }

        var menu = toolGroup.Children.FirstOrDefault(item => item.Header == toolName);
        if (menu != null)
        {
            toolGroup.Children.Remove(menu);
        }
    }

    public ObservableCollection<ToolMenuItem> MenuItems { get; set; } = new();
}