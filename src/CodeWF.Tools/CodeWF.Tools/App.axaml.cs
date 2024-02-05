using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CodeWF.Tools.ViewModels;
using CodeWF.Tools.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWF.Tools;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var locator = new ViewLocator();
        DataTemplates.Add(locator);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Configure();
            var vm = Ioc.Default.GetService<MainWindowViewModel>();
            var view = (Window)locator.Build(vm)!;
            view.DataContext = vm;

            desktop.MainWindow = view;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Configure();
            var vm = Ioc.Default.GetService<MainViewModel>();
            var view = (UserControl)locator.Build(vm)!;
            view.DataContext = vm;
            singleViewPlatform.MainView = view;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Configure()
    {
        var services = new ServiceCollection();
        ConfigureViewModels(services);
        ConfigureViews(services);
        var provider = services.BuildServiceProvider();

        Ioc.Default.ConfigureServices(provider);
    }

    private void ConfigureViewModels(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomePageViewModel>();
        services.AddTransient<TranslatePageViewModel>();
        services.AddTransient<SlugGeneratorPageViewModel>();
    }

    private void ConfigureViews(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainView>();
        services.AddTransient<HomePageView>();
        services.AddTransient<TranslatePageView>();
        services.AddTransient<SlugGeneratorPageView>();
    }
}