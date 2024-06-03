using CodeWF.WebAPI.Options;
using CodeWF.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CodeWF.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController(
    ILogger<HomeController> logger,
    IOptions<SiteOption> siteOptions,
    IMemoryCache memoryCache)
    : ControllerBase
{
    /// <summary>
    ///     获取所有友情链接
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetBase")]
    public async Task<SiteBase?> GetAsync()
    {
        var baseInfo = new SiteBase
        {
            Base = new SiteInfo
            {
                Name = siteOptions.Value.Name,
                Memo = siteOptions.Value.Memo,
                Logo = siteOptions.Value.Logo,
                Owner = siteOptions.Value.Owner,
                OwnerWeChat = siteOptions.Value.OwnerWeChat,
                WeChatPublic = siteOptions.Value.WeChatPublic,
                Start = siteOptions.Value.Start,
                ToolUrl = siteOptions.Value.ToolUrl,
                BlogPostUrl = siteOptions.Value.BlogPostUrl
            },
            Tools = new List<ToolItem>
            {
                new()
                {
                    Name = "二维码生成器", Memo = "生成并下载url或文本的QR代码，并自定义背景和前景颜色。",
                    Url = "https://tools.dotnet9.com/qrcode-generator",
                    Cover = "https://img1.dotnet9.com/site/wechatpublic.jpg"
                },

                new()
                {
                    Name = "日期时间转换器", Memo = "将日期和时间转换为各种不同的格式",
                    Url = "https://tools.dotnet9.com/date-converter",
                    Cover = "https://img1.dotnet9.com/site/wechatpublic.jpg"
                },

                new()
                {
                    Name = "整数基转换器", Memo = "在不同的基数（十进制、十六进制、二进制、八进制、base64…）之间转换数字",
                    Url = "https://tools.dotnet9.com/base-converter",
                    Cover = "https://img1.dotnet9.com/site/wechatpublic.jpg"
                },
                new()
                {
                    Name = "打乱字符串", Memo = "确保字符串 url、文件名和 id 安全。",
                    Url = "https://tools.dotnet9.com/slugify-string",
                    Cover = "https://img1.dotnet9.com/site/wechatpublic.jpg"
                }
            },
            BlogPosts = new List<BlogPostItem>
            {
                new()
                {
                    Title = "NetBeauty2：让你的.NET项目输出目录更清爽",
                    Memo =
                        "在.NET项目开发中，随着项目复杂性的增加，依赖的dll文件也会逐渐增多。这往往导致输出目录混乱，不便于管理和部署。而NetBeauty2开源项目正是为了解决这一问题而生，它能够帮助开发者在独立发布.NET项目时，将.NET运行时和依赖的dll文件移动到指定的目录，从而让输出目录更加干净、清爽。",
                    Cover = "https://img1.dotnet9.com/2024/03/cover_06.png",
                    Url =
                        "https://blog.dotnet9.com/post/2024/3/12/netbeauty2-let-your-dotnet-project-output-directory-is-more-refreshing"
                },
                new()
                {
                    Title = ".NET跨平台客户端框架 - Avalonia UI",
                    Memo =
                        "这是一个基于WPF XAML的跨平台UI框架，并支持多种操作系统（Windows（.NET Core），Linux（GTK），MacOS，Android和iOS），Web（WebAssembly）",
                    Cover = "https://img1.dotnet9.com/2022/11/0402.png",
                    Url =
                        "https://blog.dotnet9.com/post/2022/11/19/one-of-the-best-choices-for-dotnet-cross-platform-frameworks-avalonia-ui"
                },
                new()
                {
                    Title = "各版本操作系统对.NET支持情况",
                    Memo = "借助虚拟机和测试机，检测各版本操作系统对.NET的支持情况。安装操作系统后，实测安装相应运行时并能够运行星尘代理为通过。",
                    Cover = "https://img1.dotnet9.com/2024/01/cover_02.png",
                    Url =
                        "https://blog.dotnet9.com/post/2024/1/13/each-version-of-the-operating-system-is-correct-dotnet-support"
                },
                new()
                {
                    Title = ".NET反编译、第三方库调试（拦截、篡改、伪造）、一库多版本兼容",
                    Memo = "使用者本人对于传播和利用本公众号提供的信息所造成的任何直接或间接的后果和损失负全部责任。公众号及作者对于这些后果不承担任何责任。如果造成后果，请自行承担责任。谢谢！",
                    Cover = "https://img1.dotnet9.com/2023/09/cover_08.png",
                    Url =
                        "https://blog.dotnet9.com/post/2023/9/26/simulate-scenarios-using-third-party-dotnet-libraries-for-debugging-interception-decompilation-and-compatibility-solutions-for-multiple-versions-of-one-library"
                }
            }
        };

        return baseInfo;
    }
}