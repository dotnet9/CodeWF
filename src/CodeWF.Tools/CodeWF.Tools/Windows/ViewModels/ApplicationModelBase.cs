namespace CodeWF.Tools.Windows.ViewModels;

internal abstract class ApplicationModelBase : ViewModelBase, IMainWindowState
{
	private readonly IThemeSwitch _themeSwitch;

	private bool _aboutEnable;

	public bool AboutEnabled
	{
		get => _aboutEnable;
		set => this.RaiseAndSetIfChanged(ref _aboutEnable, value);
	}

	private bool _darkThemeEnabled = false;

	public bool DarkThemeEnabled
	{
		get => _darkThemeEnabled;
		set => this.RaiseAndSetIfChanged(ref _darkThemeEnabled, value);
	}

	private int _selectedPageIndex = 0;

	public int CurrentPageIndex
	{
		get => _selectedPageIndex;
		set => this.RaiseAndSetIfChanged(ref _selectedPageIndex, value);
	}

	private bool _isDialogOpened;

	public bool IsDialogOpened
	{
		get => _isDialogOpened;
		set => this.RaiseAndSetIfChanged(ref _isDialogOpened, value);
	}

	protected ApplicationModelBase(IThemeSwitch themeSwitch)
	{
		AboutEnabled = true;
		var theme = Application.Current!.GetValue(ThemeVariantScope.ActualThemeVariantProperty);

		_themeSwitch = themeSwitch;

		var appTheme = theme == ThemeVariant.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
		InitializeTheme(appTheme);
	}

	public abstract void HelpAboutMethod();
	public abstract void AppExitCommand();
	public abstract void SwitchThemeCommand(bool dark);

	protected async void RunHelpAbout(Window currentWindow)
	{
		if (AboutEnabled)
		{
			try
			{
				AboutEnabled = false;
				await new AboutWindow(IsDarkTheme(_themeSwitch.Current)).ShowDialog(currentWindow);
			}
			finally
			{
				AboutEnabled = true;
			}
		}
	}

	protected void AppExit()
	{
		if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.Shutdown(0);
		}
	}


	protected void SetTheme(ApplicationTheme theme)
	{
		InitializeTheme(theme);
		_themeSwitch.ChangeTheme(theme);
	}


	private void InitializeTheme(ApplicationTheme theme)
	{
		DarkThemeEnabled = (theme == ApplicationTheme.Dark);
	}

	private static bool IsDarkTheme(ApplicationTheme? theme)
		=> theme switch
		{
			ApplicationTheme.Dark => true,
			_ => false,
		};
}