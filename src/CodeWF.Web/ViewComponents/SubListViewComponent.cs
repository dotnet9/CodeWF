namespace CodeWF.Web.ViewComponents;

public class SubListViewComponent(ILogger<SubListViewComponent> logger, IMediator mediator) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            IReadOnlyList<Category> cats = await mediator.Send(new GetCategoriesQuery());
            IEnumerable<KeyValuePair<string, string>> items = cats.Select(c =>
                new KeyValuePair<string, string>(c.DisplayName, c.RouteName));

            return View(items);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return Content("ERROR");
        }
    }
}