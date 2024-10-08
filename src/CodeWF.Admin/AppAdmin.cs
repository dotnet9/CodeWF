namespace CodeWF.Admin;

/// <summary>
/// CodeWF管理端配置类。
/// </summary>
public static class AppAdmin
{
    /// <summary>
    /// 添加CodeWF管理端应用。
    /// </summary>
    /// <param name="services">依赖注入服务。</param>
    public static void AddCodeWFAdmin(this IServiceCollection services)
    {
        ModuleHelper.InitAppModules();
        //Stopwatcher.Enabled = true;
        services.AddCodeWF(option => option.IsSite = false);
        services.AddKnownAntDesign();

        services.AddScoped<IHomeService, HomeService>();
        services.AddScoped<ICommonService, CommonService>();
        services.AddScoped<IPostService, PostService>();

        //注册待办事项显示流程表单
        //Config.ShowMyFlow = flow =>
        //{
        //    if (flow.Flow.FlowCode == AppFlow.Apply.Code)
        //        ApplyForm.ShowMyFlow(flow);
        //};

        //添加模块
        Config.DbAssemblies.Add(typeof(AppAdmin).Assembly);
        Config.AddModule(typeof(AppAdmin).Assembly);
    }
}