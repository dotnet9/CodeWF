using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Tool.Web;

public class SlugifyStringModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SlugifyStringModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string InputString { get; set; } = string.Empty;
    public string TranslatedResult { get; set; } = string.Empty;
    public string SlugResult { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task OnPostAsync()
    {
        InputString = Request.Form["InputString"].ToString();

        if (string.IsNullOrWhiteSpace(InputString))
        {
            TranslatedResult = string.Empty;
            SlugResult = string.Empty;
            return;
        }

        var translatedText = await TranslateToEnglishAsync(InputString.Trim());
        TranslatedResult = translatedText;
        SlugResult = Slugify(translatedText);
    }

    private async Task<string> TranslateToEnglishAsync(string text)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var encodedText = Uri.EscapeDataString(text);
            var url = $"https://api.mymemory.translated.net/get?q={encodedText}&langpair=zh-CN|en";

            var response = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            if (doc.RootElement.TryGetProperty("responseData", out var responseData) &&
                responseData.TryGetProperty("translatedText", out var translatedText))
            {
                return translatedText.GetString() ?? text;
            }

            return text;
        }
        catch
        {
            return text;
        }
    }

    private static string Slugify(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var slug = input.ToLowerInvariant();
        slug = slug.Replace(' ', '-');
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }
}
