namespace CodeWF.Tools
{
	public partial class App : Application, IThemeSwitch
	{
		private ApplicationTheme _currentTheme;

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
			Name = "码界工坊";
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				IOC();
				desktop.MainWindow = new MainWindow();
				DataContext = desktop.MainWindow.DataContext;
			}

			base.OnFrameworkInitializationCompleted();
		}

		ApplicationTheme IThemeSwitch.Current => this._currentTheme;


		void IThemeSwitch.ChangeTheme(ApplicationTheme theme)
		{
			if (theme == this._currentTheme)
				return;

			_currentTheme = theme;

			var neumorphismTheme = Application.Current!.LocateMaterialTheme<NeumorphismTheme>();
			if (neumorphismTheme != null)
			{
				neumorphismTheme.BaseTheme = theme;
			}
		}

		/// <summary>
		/// 注入工具需要的服务
		/// </summary>
		private void IOC()
		{
			Locator.CurrentMutable.RegisterConstant(new TranslationService(), typeof(ITranslationService));
		}
	}
}