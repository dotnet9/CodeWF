namespace CodeWF.Tools.Modules.Web;

public class WebModule : IModule
{
    public WebModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Web, WebToolInfo.SlugifyName, WebToolInfo.SlugifyDescription,
            nameof(SlugifyView),
            IconHelper.SlugifyName,
            ToolStatus.Complete);

        toolManagerService.AddTool(ToolType.Web, WebToolInfo.IPQueryName, WebToolInfo.IPQueryDescription,
            nameof(IPQueryView),
            IconHelper.IPQuery,
            ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<SlugifyView>(RegionNames.ContentRegion);
        regionManager.RegisterViewWithRegion<IPQueryView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register(typeof(ITranslationService), typeof(TranslationService));
        containerRegistry.RegisterSingleton(typeof(SlugifyViewModel));
        containerRegistry.RegisterSingleton(typeof(IPQueryViewModel));
    }
}