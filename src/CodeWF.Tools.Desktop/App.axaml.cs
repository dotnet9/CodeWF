namespace CodeWF.Tools.Desktop;

public class App : PrismApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize(); // <-- Required
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);

        moduleCatalog.AddModule<DeveloperModule>();
        moduleCatalog.AddModule<WebModule>();
        moduleCatalog.AddModule<TestModule>();
    }

    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);

        regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
        regionAdapterMappings.RegisterMapping(typeof(Grid), Container.Resolve<GridRegionAdapter>());
        regionAdapterMappings.RegisterMapping(typeof(TabControl), Container.Resolve<TabControlAdapter>());
    }

    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var container = containerRegistry.GetContainer();
        // Views - Generic
        containerRegistry.Register<MainWindow>();

        var regionManager = Container.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<DashboardView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<FooterView>(RegionNames.FooterRegion);

        containerRegistry.RegisterSingleton(typeof(FooterViewModel));
        containerRegistry.RegisterSingleton(typeof(DashboardViewModel));

        containerRegistry.RegisterSingleton<INotificationService, NotificationService>();
        containerRegistry.RegisterSingleton<IClipboardService, ClipboardService>();
        containerRegistry.RegisterSingleton<IToolManagerService, ToolManagerService>();

        var toolManagerService = container.Resolve<IToolManagerService>();
        toolManagerService.AddTool("首页",
            "这是一个基于Avalonia UI + Prism框架打造的模块化跨平台工具平台，汇聚了众多开发实用小工具，目前已开发或即将开发的如编码解码、数据加密等，轻量且强大，开箱即用，助力开发者提升工作效率。",
            nameof(DashboardView), ToolStatus.Complete);
    }

    /// <summary>
    /// 1、DryIoc.Microsoft.DependencyInjection低版本可不要这个方法（5.1.0及以下）
    /// 2、高版本必须，否则会抛出异常：System.MissingMethodException:“Method not found: 'DryIoc.Rules DryIoc.Rules.WithoutFastExpressionCompiler()'.”
    /// 参考issues：https://github.com/dadhi/DryIoc/issues/529
    /// </summary>
    /// <returns></returns>
    protected override Rules CreateContainerRules()
    {
        return Rules.Default.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient)
            .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
            .WithFuncAndLazyWithoutRegistration()
            .WithTrackingDisposableTransients()
            //.WithoutFastExpressionCompiler()
            .WithFactorySelector(Rules.SelectLastRegisteredFactory());
    }

    protected override IContainerExtension CreateContainerExtension()
    {
        IContainer container = new Container(CreateContainerRules());
        container.WithDependencyInjectionAdapter();

        return new DryIocContainerExtension(container);
    }

    protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
    {
        base.RegisterRequiredTypes(containerRegistry);

        IServiceCollection services = ConfigureServices();

        IContainer container = ((IContainerExtension<IContainer>)containerRegistry).Instance;

        container.Populate(services);
    }

    private static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // 注入MediatR
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        // 添加模块注入，未显示调用模块类型前，模块程序集是未加载到当前程序域`AppDomain.CurrentDomain`的
        var assembly = typeof(TestModule).GetAssembly();
        assemblies.Add(assembly);
        services.AddMediatR(configure =>
        {
            configure.RegisterServicesFromAssemblies(assemblies.ToArray());
        });

        return services;
    }
}