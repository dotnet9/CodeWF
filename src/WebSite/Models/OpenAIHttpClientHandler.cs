using Microsoft.Extensions.Options;
using WebSite.Options;

namespace WebSite.Models;

public class OpenAIHttpClientHandler(IOptions<OpenAIOption> openAiOption) : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string url = openAiOption.Value.Endpoint!;
        Uri uri = new Uri(url);
        string host = uri.Host;
        switch (request.RequestUri?.LocalPath)
        {
            case "/v1/chat/completions":
                UriBuilder uriBuilder = new(request.RequestUri)
                {
                    Scheme = "https",
                    Host = host,
                    Path = "v1/chat/completions",
                };
                request.RequestUri = uriBuilder.Uri;
                break;
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}