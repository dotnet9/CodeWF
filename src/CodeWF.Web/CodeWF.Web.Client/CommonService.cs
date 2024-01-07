namespace CodeWF.Web.Client;

public static class CommonService
{
	/// <summary>
	/// 注册通用服务，比如Masa Blazor的组件等，客户端工程和主工程都需要注册
	/// </summary>
	/// <param name="services"></param>
	public static void RegisterCommonService(this IServiceCollection services)
	{
		services.AddSingleton<ITranslationService, TranslationService>();
		services.AddMasaBlazor(options =>
		{
			options.ConfigureIcons(IconSet.MaterialDesign);
			options.ConfigureSsr(ssr =>
			{
				ssr.Left = 256;
				ssr.Top = 64;
			});
		});
	}
}