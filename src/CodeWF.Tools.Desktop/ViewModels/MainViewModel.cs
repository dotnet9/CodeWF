using ReactiveUI;
using System.Globalization;

namespace CodeWF.Tools.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager;

    public ObservableCollection<ToolMenuItem> SearchMenuItems { get; set; }
    private ToolMenuItem? _searchSelectedItem;

    public ToolMenuItem? SearchSelectedItem
    {
        get => _searchSelectedItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchSelectedItem, value);
            ChangeSearchMenu();
        }
    }

    private ToolMenuItem? _selectedMenuItem;

    public ToolMenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            ChangeTool();
        }
    }

    private NotificationType _selectedMenuStatus;

    public NotificationType SelectedMenuStatus
    {
        get => _selectedMenuStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuStatus, value);
    }

    public ObservableCollection<ToolMenuItem> MenuItems { get; private set; }

    public MainViewModel(IToolManagerService toolManagerService, INotificationService notificationService,
        IEventAggregator eventAggregator, IRegionManager regionManager)
    {
        Title = AppInfo.Name;
        _notificationService = notificationService;
        _eventAggregator = eventAggregator;
        _regionManager = regionManager;
        RegisterPrismEvent();
        SearchMenuItems = new ObservableCollection<ToolMenuItem>();
        MenuItems = toolManagerService.MenuItems;
        MenuItems.CollectionChanged += ToolListChanged;
    }

    private void RegisterPrismEvent()
    {
        _eventAggregator.GetEvent<TestEvent>().Subscribe(args =>
        {
            _notificationService?.Show("Prism Event",
                $"主工程Prism事件处理程序：Args = {args.Args}, Now = {DateTime.Now}");
        });
    }

    private void ToolListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SearchMenuItems.Clear();
        MenuItems.ForEach(firstMenuItem =>
        {
            if (firstMenuItem.Children.Any())
            {
                firstMenuItem.Children.ForEach(secondMenuItem => SearchMenuItems.Add(secondMenuItem));
            }
        });
        SelectedMenuItem = SelectedMenuItem == null ? MenuItems.First() : GetMenuItem(SelectedMenuItem.Header!);
    }

    private ToolMenuItem? GetMenuItem(string name)
    {
        foreach (var firstMenuItem in MenuItems)
        {
            if (name == firstMenuItem.Header)
            {
                return firstMenuItem;
            }

            foreach (var secondMenuItem in firstMenuItem.Children)
            {
                if (name == secondMenuItem.Header)
                {
                    return secondMenuItem;
                }
            }
        }

        return default;
    }

    private void ChangeTool()
    {
        _regionManager.RequestNavigate(RegionNames.ContentRegion, _selectedMenuItem?.ViewName);
        SelectedMenuStatus = _selectedMenuItem?.Status switch
        {
            ToolStatus.Planned => NotificationType.Warning,
            ToolStatus.Developing => NotificationType.Information,
            ToolStatus.Complete => NotificationType.Success,
            _ => NotificationType.Information
        };
    }

    private void ChangeSearchMenu()
    {
        foreach (var firstMenuItem in MenuItems)
        {
            foreach (var secondMenuItem in firstMenuItem.Children)
            {
                if (secondMenuItem.Header != _searchSelectedItem?.Header)
                {
                    continue;
                }

                SelectedMenuItem = secondMenuItem;
                return;
            }
        }
    }
}