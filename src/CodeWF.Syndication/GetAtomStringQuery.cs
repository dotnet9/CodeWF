namespace CodeWF.Syndication;

public record GetAtomStringQuery(string CategoryName = null) : IRequest<string>;

public class GetAtomStringQueryHandler : IRequestHandler<GetAtomStringQuery, string>
{
    private readonly FeedGenerator _feedGenerator;
    private readonly ISyndicationDataSource _sdds;

    public GetAtomStringQueryHandler(IBlogConfig blogConfig, ISyndicationDataSource sdds,
        IHttpContextAccessor httpContextAccessor)
    {
        _sdds = sdds;

        IHttpContextAccessor acc = httpContextAccessor;
        string baseUrl = $"{acc.HttpContext.Request.Scheme}://{acc.HttpContext.Request.Host}";

        _feedGenerator = new FeedGenerator(
            baseUrl,
            blogConfig.GeneralSettings.SiteTitle,
            blogConfig.GeneralSettings.Description,
            Helper.FormatCopyright2Html(blogConfig.GeneralSettings.Copyright).Replace("&copy;", "©"),
            $"CodeWF v{Helper.AppVersion}",
            baseUrl,
            blogConfig.GeneralSettings.DefaultLanguageCode);
    }

    public async Task<string> Handle(GetAtomStringQuery request, CancellationToken ct)
    {
        IReadOnlyList<FeedEntry>? data = await _sdds.GetFeedDataAsync(request.CategoryName);
        if (data is null)
        {
            return null;
        }

        _feedGenerator.FeedItemCollection = data;
        string xml = await _feedGenerator.WriteAtomAsync();
        return xml;
    }
}