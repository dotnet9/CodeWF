namespace CodeWF.Tools.Module.Footer;

public class FooterModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion(RegionNames.FooterRegion, typeof(FooterView));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(FooterViewModel));
    }
}