namespace CodeWF.Pingback;

public interface IPingSourceInspector
{
    Task<PingRequest> ExamineSourceAsync(string sourceUrl, string targetUrl);
}

public class PingSourceInspector(ILogger<PingSourceInspector> logger, HttpClient httpClient) : IPingSourceInspector
{
    public async Task<PingRequest> ExamineSourceAsync(string sourceUrl, string targetUrl)
    {
        try
        {
            Regex regexHtml = new Regex(
                @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>",
                RegexOptions.Singleline | RegexOptions.Compiled);

            Regex regexTitle = new Regex(
                @"(?<=<title.*>)([\s\S]*)(?=</title>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string html = await httpClient.GetStringAsync(sourceUrl);
            string title = regexTitle.Match(html).Value.Trim();
            bool containsHtml = regexHtml.IsMatch(title);
            bool sourceHasLink = html.ToUpperInvariant().Contains(targetUrl.ToUpperInvariant());

            PingRequest pingRequest = new PingRequest
            {
                Title = title,
                ContainsHtml = containsHtml,
                SourceHasLink = sourceHasLink,
                TargetUrl = targetUrl,
                SourceUrl = sourceUrl
            };

            return pingRequest;
        }
        catch (WebException ex)
        {
            logger.LogError(ex, nameof(ExamineSourceAsync));
            return null;
        }
    }
}