

namespace CodeWF.Tools.Modules.Web;

public class WebModule : IModule
{
    public WebModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Web, WebToolInfo.SlugifyName, WebToolInfo.SlugifyDescription,
            nameof(SlugifyView),
            IconHelper.SlugifyName,
            ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<SlugifyView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register(typeof(ITranslationService), typeof(TranslationService));
        containerRegistry.RegisterSingleton(typeof(SlugifyViewModel));
    }
}