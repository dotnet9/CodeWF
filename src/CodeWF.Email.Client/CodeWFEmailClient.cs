using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CodeWF.Email.Client;

public interface ICodeWFEmailClient
{
    Task SendEmail<T>(MailMesageTypes type, string[] receipts, T payload) where T : class;
}

public class CodeWFEmailClient : ICodeWFEmailClient
{
    private readonly IBlogConfig _blogConfig;

    private readonly bool _enabled;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CodeWFEmailClient> _logger;

    public CodeWFEmailClient(IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<CodeWFEmailClient> logger,
        HttpClient httpClient,
        IBlogConfig blogConfig)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _httpClient = httpClient;
        _blogConfig = blogConfig;

        if (!string.IsNullOrWhiteSpace(configuration["Email:ApiEndpoint"]))
        {
            _httpClient.BaseAddress = new Uri(configuration["Email:ApiEndpoint"]);
            _httpClient.DefaultRequestHeaders.Add("x-functions-key", configuration["Email:ApiKey"]);
            _enabled = true;
        }
        else
        {
            _logger.LogError("Email:ApiEndpoint is empty");
            _enabled = false;
        }
    }

    /// <summary>
    ///     Send email to `/api/enqueue` endpoint
    /// </summary>
    public async Task SendEmail<T>(MailMesageTypes type, string[] receipts, T payload) where T : class
    {
        if (!_blogConfig.NotificationSettings.EnableEmailSending || !_enabled)
        {
            return;
        }

        try
        {
            EmailNotification en = new EmailNotification
            {
                Type = type.ToString(),
                Receipts = receipts,
                Payload = payload,
                OriginAspNetRequestId = _httpContextAccessor.HttpContext?.TraceIdentifier
            };

            // Note: Do not use `PostAsJsonAsync` here, Azure Function will blow up on encoded http request
            string json = JsonSerializer.Serialize(en);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/api/enqueue", content);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation($"Email sent: {json}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}