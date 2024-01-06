namespace CodeWF.Desktop.Windows;

public sealed partial class AboutWindow : Window
{
	private readonly bool _darkTheme;

	public AboutWindow() : this(false)
	{
	}

	public AboutWindow(bool darkTheme)
	{
		_darkTheme = darkTheme;

		InitializeComponent();

		DataContext = new AboutViewModel(_darkTheme);
	}
}