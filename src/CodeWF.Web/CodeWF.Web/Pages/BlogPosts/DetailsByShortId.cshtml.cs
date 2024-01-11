namespace CodeWF.Web.Pages.BlogPosts;

public class DetailsByShortIdModel : PageModel
{
    private readonly IDistributedCacheHelper _cacheHelper;
    private readonly IEventBus _eventBus;
    private readonly IBlogPostService _service;
    private readonly CodeWFDbContext _dbContext;

    public DetailsByShortIdModel(IBlogPostService service, CodeWFDbContext dbContext, IEventBus eventBus,
        IDistributedCacheHelper cacheHelper)
    {
        _service = service;
        _dbContext = dbContext;
        _eventBus = eventBus;
        _cacheHelper = cacheHelper;
    }

    [BindProperty(SupportsGet = true)] public string ShortId { get; set; } = null!;
    public BlogPostDetails? Current { get; set; }
    public string? ContentHtml { get; set; }


    public async Task OnGet()
    {
        string cacheKey = $"BlogPost_ShortId_{ShortId}";

        async Task<BlogPostDetailsViewModel?> GetBlogPostFromDb()
        {
            var vm = new BlogPostDetailsViewModel();
            vm.Current = await _service.BlogPostDetailsByShortIdAsync(ShortId);

            return vm;
        }

        var vm = await _cacheHelper.GetOrCreateAsync(cacheKey,
            async e => await GetBlogPostFromDb());
        Current = vm?.Current;
        ContentHtml = Current?.Content.Convert2Html();
        if (Current != null)
        {
            _eventBus.Publish("CodeWF.Web.BlogPosts.OnGet", new ReadBlogPostEvent(Current!.Slug));
        }
    }
}