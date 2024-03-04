namespace CodeWF.Tools.Desktop.ViewModels;

public class MainWindowViewModel(ISender sender, IPublisher publisher) : ViewModelBase(sender, publisher)
{
    public string Title => "Dotnet工具箱";
}