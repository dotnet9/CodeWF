using CodeWF.Tools.Desktop.MediatR;
using DryIoc;
using DryIoc.Messages;
using MediatR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Prism.Ioc;
using System.Net.NetworkInformation;
using System.Reflection;

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

        moduleCatalog.AddModule<SlugifyStringModule>();
        moduleCatalog.AddModule<FooterModule>();
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

        // Views - Region Navigation

        //×¢²áMediatR
        container.RegisterMany(
            [AppDomain.CurrentDomain.Load("CodeWF.Tools.MediatR")], Registrator.Interfaces);
        container.Register(typeof(IRequestHandler<,>), typeof(CopyToClipboardCommandHandle), setup: Setup.Decorator);
        container.Register<IMediator, Mediator>(made: Made.Of(() => new Mediator(Arg.Of<IServiceProvider>())));
    }
}