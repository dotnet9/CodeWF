namespace CodeWF.Tools.Desktop.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IToolManagerService _toolManagerService;
    private readonly IEventAggregator _eventAggregator;

    public ObservableCollection<ToolMenuItem> MenuItems { get; } =
        new ObservableCollection<ToolMenuItem>();

    public DashboardViewModel(IToolManagerService toolManagerService, IEventAggregator eventAggregator)
    {
        _toolManagerService = toolManagerService;
        _eventAggregator = eventAggregator;
        toolManagerService.ToolMenuChanged += MenuChangedHandler;
    }

    private void MenuChangedHandler(object sender, EventArgs e)
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

    public void ExecuteChangeToolHandle(ToolMenuItem menuItem)
    {
        _eventAggregator.GetEvent<ChangeToolEvent>()
            .Publish(new ChangeToolEventParameter() { ToolHeader = menuItem.Header });
    }
}