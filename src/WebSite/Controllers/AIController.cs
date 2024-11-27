using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using WebSite.Models;
using WebSite.Options;
using WebSite.ViewModels;

namespace WebSite.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AIController
    : ControllerBase
{
    private readonly Kernel _kernel;

    public AIController(IOptions<OpenAIOption> openAIOption, OpenAIHttpClientHandler httpClientHandler,
        ILogger<AIController> logger)
    {
        httpClientHandler.ServerCertificateCustomValidationCallback += (_, _, _, _) => true;
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: openAIOption.Value.ChatModel!,
                apiKey: openAIOption.Value.Key!,
                httpClient: new HttpClient(httpClientHandler));
        _kernel = builder.Build();
    }

    [HttpPost]
    public async Task<IResult> AskBotAsync([FromBody] AskBotRequest request)
    {
        var content = _kernel.InvokePromptStreamingAsync(request.Content);
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    [HttpPost]
    public async Task<IResult> PolyTranslateAsync([FromBody] PolyTranslateRequest request)
    {
        const string skPrompt = """  
                                {{$input}}  

                                将上面的输入翻译成{{$language}}，无需任何其他内容  
                                """;
        var content = _kernel.InvokePromptStreamingAsync(skPrompt,
            new KernelArguments { ["input"] = request.Content, ["language"] = string.Join(",", request.Languages) });
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    [HttpPost]
    public async Task<IResult> Title2SlugAsync([FromBody] Title2SlugRequest request)
    {
        const string skPrompt = """  
                                {{$input}}  

                                - 将上面的输入内容转换翻译成英文Url Slug，不要进行回复我，只需要提供转换后的内容
                                """;
        var content = _kernel.InvokePromptStreamingAsync(skPrompt,
            new KernelArguments { ["input"] = request.Content });
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    private async Task WriteResponseAsync(IAsyncEnumerable<StreamingKernelContent> content)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        await Response.Body.FlushAsync();

        await foreach (var item in content)
        {
            await Response.WriteAsync(item.ToString());
            await Response.Body.FlushAsync();
        }

        await Response.Body.FlushAsync();
    }
}