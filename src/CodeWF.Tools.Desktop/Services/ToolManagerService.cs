namespace CodeWF.Tools.Desktop.Services;

internal class ToolManagerService : IToolManagerService
{
    public void AddTool(string name, string description, string viewName)
    {
        MenuItems.Add(new ToolMenuItem
        {
            Group = ToolType.Home,
            Level = 0,
            Header = name,
            Description = description,
            ViewName = viewName
        });
    }

    public void AddTool(ToolType group, string name, string description, string viewName)
    {
        var toolGroup = MenuItems.FirstOrDefault(item => item.Group == group);
        if (toolGroup == null)
        {
            var desc = group.Description();
            toolGroup = new ToolMenuItem() { Group = group, Level = 0, Header = desc, Description = desc };
            MenuItems.Add(toolGroup);
        }

        toolGroup.Children.Add(new ToolMenuItem
        {
            Group = group,
            Level = 1,
            Header = name,
            Description = description,
            ViewName = viewName
        });
    }

    public void RemoveTool(string name)
    {
        for (var i = MenuItems.Count; i >= 0; i--)
        {
            if (MenuItems[i].Header == name)
                MenuItems.RemoveAt(i);
            var firstMenuItem = MenuItems[i];
            for (int j = 0; j < firstMenuItem.Children.Count; j++)
            {
                if (firstMenuItem.Children[j].Header == name)
                {
                    firstMenuItem.Children.RemoveAt(j);
                }
            }
        }
    }

    public void RemoveTool(ToolType type, string toolName)
    {
    }

    public ObservableCollection<ToolMenuItem> MenuItems { get; set; } = new();
}