namespace CodeWF.Web.ViewComponents;

public class MenuViewComponent(IBlogConfig blogConfig, ILogger<MenuViewComponent> logger) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        try
        {
            CustomMenuSettings settings = blogConfig.CustomMenuSettings;
            return View(settings);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return Content("ERROR");
        }
    }
}