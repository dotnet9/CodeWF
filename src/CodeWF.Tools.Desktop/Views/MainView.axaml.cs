namespace CodeWF.Tools.Desktop.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var level = TopLevel.GetTopLevel(this);
        if (level == null)
        {
            return;
        }

        var notificationService = ContainerLocator.Current.Resolve<INotificationService>();
        notificationService.SetHostWindow(level);
    }
}