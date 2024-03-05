namespace CodeWF.Tools.Desktop.ViewModels;

internal class DashboardViewModel : ViewModelBase
{
    private readonly IToolManagerService _toolManagerService;

    public ObservableCollection<ToolMenuItem> MenuItems { get; } =
        new ObservableCollection<ToolMenuItem>();

    public DashboardViewModel(IToolManagerService toolManagerService)
    {
        _toolManagerService = toolManagerService;
        toolManagerService.MenuItems.CollectionChanged += ToolListChanged;
    }

    private void ToolListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MenuItems.Clear();
        _toolManagerService.MenuItems.ForEach(firstMenuItem =>
        {
            if (firstMenuItem.Children.Any())
            {
                MenuItems.Add(firstMenuItem);
            }
        });
    }
}