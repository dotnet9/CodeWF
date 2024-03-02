namespace CodeWF.Tools.Desktop.ViewModels;

public class MainWindowViewModel(ISender sender, IPublisher publisher) : ViewModelBase(sender, publisher)
{
    public string Title => "工具箱-码界工坊";
}