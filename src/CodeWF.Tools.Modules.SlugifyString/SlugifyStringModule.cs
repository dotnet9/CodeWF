namespace CodeWF.Tools.Modules.SlugifyString;

public class SlugifyStringModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(SlugifyView));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register(typeof(ITranslationService), typeof(TranslationService));
        containerRegistry.RegisterSingleton(typeof(SlugifyViewModel));
    }
}