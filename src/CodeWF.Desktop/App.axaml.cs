using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CodeWF.Core;
using CodeWF.Desktop.ViewModels;
using CodeWF.Desktop.Views;
using Splat;

namespace CodeWF.Desktop
{
	public partial class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				IOC();
				desktop.MainWindow = new MainWindow
				{
					DataContext = new MainWindowViewModel(),
				};
			}

			base.OnFrameworkInitializationCompleted();
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