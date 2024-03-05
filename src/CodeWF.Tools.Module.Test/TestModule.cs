namespace CodeWF.Tools.Module.Test;

public class TestModule : IModule
{
    public TestModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Test, "测试使用",
            "测试AvaloniaUI中使用Prism及MediatR",
            nameof(TestView), ToolStatus.Complete);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion<TestView>(RegionNames.ContentRegion);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(TestViewModel));
    }
}