using CodeWF.Tools.Core.Services;

namespace CodeWF.Tools.Modules.Web;

public class WebModule : IModule
{
    public WebModule(IToolManagerService toolManagerService)
    {
        toolManagerService.AddTool(ToolType.Web, "别名转换",
            "这款工具能迅速将中文文章标题译成英文，并自动转换为适合浏览器的URL格式，省去了手动调整和编码的繁琐，极大提升了内容管理和网站国际化的效率，是多语言环境下内容创建与发布的得力助手。",
            nameof(SlugifyView), ToolStatus.Complete);
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