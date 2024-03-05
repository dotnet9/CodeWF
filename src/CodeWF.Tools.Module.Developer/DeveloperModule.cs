namespace CodeWF.Tools.Module.Developer;

public class DeveloperModule : IModule
{
    public DeveloperModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Developer, "时间戳转换",
            "Unix 时间戳是从1970年1月1日（UTC/GMT的午夜）开始所经过的秒数，不考虑闰秒。",
            nameof(TimestampView), ToolStatus.Developing);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<TimestampView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(TimestampViewModel));
    }
}