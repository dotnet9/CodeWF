namespace CodeWF.Web.ViewComponents;

public class TagsViewComponent(IBlogConfig blogConfig, IMediator mediator, ILogger<SubListViewComponent> logger)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            IReadOnlyList<KeyValuePair<Core.TagFeature.Tag, int>> tags =
                await mediator.Send(new GetHotTagsQuery(blogConfig.GeneralSettings.HotTagAmount));
            return View(tags);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return Content("ERROR");
        }
    }
}