namespace CodeWF.Pingback;

public class PingbackSender(
    HttpClient httpClient,
    IPingbackWebRequest pingbackWebRequest,
    ILogger<PingbackSender> logger = null)
    : IPingbackSender
{
    private static readonly Regex UrlsRegex = new(
        @"<a.*?href=[""'](?<url>.*?)[""'].*?>(?<name>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task TrySendPingAsync(string postUrl, string postContent)
    {
        try
        {
            Uri uri = new Uri(postUrl);
            string content = postContent.ToUpperInvariant();
            if (content.Contains("HTTP://") || content.Contains("HTTPS://"))
            {
                logger?.LogInformation("URL is detected in post content, trying to send ping requests.");

                foreach (Uri url in GetUrlsFromContent(postContent))
                {
                    logger?.LogInformation("Pinging URL: " + url);
                    try
                    {
                        await SendAsync(uri, url);
                    }
                    catch (Exception e)
                    {
                        logger?.LogError(e, "SendAsync Ping Error.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"{nameof(TrySendPingAsync)}({postUrl})");
        }
    }

    private async Task SendAsync(Uri sourceUrl, Uri targetUrl)
    {
        if (sourceUrl is null || targetUrl is null)
        {
            return;
        }

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(targetUrl);

            (string? key, IEnumerable<string>? value) = response.Headers.FirstOrDefault(
                h => h.Key.ToLower() == "x-pingback" || h.Key.ToLower() == "pingback");

            if (key is null || value is null)
            {
                logger?.LogInformation(
                    $"Pingback endpoint is not found for URL '{targetUrl}', ping request is terminated.");
                return;
            }

            string? pingUrl = value.FirstOrDefault();
            if (pingUrl is not null)
            {
                logger?.LogInformation($"Found Ping service URL '{pingUrl}' on target '{sourceUrl}'");

                bool successUrlCreation = Uri.TryCreate(pingUrl, UriKind.Absolute, out Uri? url);
                if (successUrlCreation)
                {
                    HttpResponseMessage pResponse = await pingbackWebRequest.Send(sourceUrl, targetUrl, url);
                }
                else
                {
                    logger?.LogInformation($"Invliad Ping service URL '{pingUrl}'");
                }
            }
        }
        catch (Exception e)
        {
            logger?.LogError(e, $"{nameof(SendAsync)}({sourceUrl},{targetUrl})");
        }
    }

    private static IEnumerable<Uri> GetUrlsFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentNullException(content);
        }

        List<Uri> urlsList = new List<Uri>();
        foreach (string url in
                 UrlsRegex.Matches(content).Select(myMatch => myMatch.Groups["url"].ToString().Trim()))
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                urlsList.Add(uri);
            }
        }

        return urlsList;
    }
}