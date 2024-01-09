namespace CodeWF.Desktop.Windows.ViewModels;

internal sealed class MainWindowViewModel<TWindow>(TWindow window) : ApplicationModelBase(window.ThemeSwitch)
	where TWindow : Window, IMainWindow
{
	private readonly TWindow _window = window;

	public override void HelpAboutMethod() => base.RunHelpAbout(_window);
	public override void AppExitCommand() => base.AppExit();


	public override void SwitchThemeCommand(bool dark)
	{
		base.SetTheme(dark ? ApplicationTheme.Dark : ApplicationTheme.Light);
	}
}