﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using WebSite.Options;
using WebSite.ViewModels;
#pragma warning disable SKEXP0010

namespace WebSite.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AIController
    : ControllerBase
{
    private readonly Kernel _kernel;

    public AIController(IOptions<OpenAIOption> openAIOption, ILogger<AIController> logger)
    {
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: openAIOption.Value.ChatModel!,
                apiKey: openAIOption.Value.Key!,
                endpoint:new Uri(openAIOption.Value.Endpoint));
        _kernel = builder.Build();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IResult> AskBotAsync([FromBody] AskBotRequest request)
    {
        var content = _kernel.InvokePromptStreamingAsync(request.Content);
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    [HttpPost]
    [AllowAnonymous]
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
    [AllowAnonymous]
    public async Task<IResult> Title2SlugAsync([FromBody] Title2SlugRequest request)
    {
        const string skPrompt = """  
                                {{$input}}  

                                - 将上面的输入内容转换翻译成英文Url Slug，英文全小写，单词之间用'-'连接，不要使用'_'，不要进行回复我，只需要提供转换后的内容
                                """;
        var content = _kernel.InvokePromptStreamingAsync(skPrompt,
            new KernelArguments { ["input"] = request.Content });
        await WriteResponseAsync(content);
        return Results.Empty;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IResult> ArticleSummaryAsync([FromBody] ArticleSummaryRequest request)
    {
        const string skPrompt = """  
                                {{$input}}  

                                - {{$length}}字总结上面的文章内容，不要进行回复我，只需要提供总结后的内容
                                """;
        var content = _kernel.InvokePromptStreamingAsync(skPrompt,
            new KernelArguments { ["input"] = request.Content, ["length"] = request.Length });
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