# Code Workshop

**Code Workshop - Enlightening with Articles, Empowering with Tools**

[简体中文](README.zh-CN.md) | English

## Introduction

The Code Workshop is a website developed using .NET 9 Blazor. It features technical articles, introductions to open-source projects, and the use of online tools: Code Workshop - Enlightening with Articles, Empowering with Tools.

During the development journey of this blog website, the website owner has overcome numerous obstacles. From the initial conception to the actual implementation, various development technologies have been explored, including [MVC]([ASP.NET Core MVC Overview | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-9.0)), [Razor Pages]([Introduction to Razor Pages in ASP.NET Core | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-9.0&tabs=visual-studio)), [Vue]([Vue.js - The Progressive JavaScript Framework | Vue.js (vuejs.org)](https://vuejs.org/)), [Go]([The Go Programming Language (google.com)](https://golang.google.com/)), [Blazor]([ASP.NET Core Blazor | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-9.0)) and so on. Throughout this long process, the website has undergone nearly 10 version upgrades, with each version embodying the owner's painstaking efforts and exploration. The details of this challenging journey are thoroughly documented in [Sharing Some Materials I Accumulated When Developing the Dotnet9 Blog Website - Code Workshop](https://dotnet9.com/bbs/post/2022/3/Share-some-learning-materials-I-accumulated-when-I-was-a-blog-website).

Now, after careful consideration and practical testing, the blog website has returned to using Blazor once again, adopting static SSR technology and incorporating the stylish and practical Ant Design design style. These series of improvements have led to a qualitative leap in the website's access speed, injecting new vitality into the website. Currently, the website has been successfully launched.

- Website URL: https://dotnet9.com

![](https://img1.dotnet9.com/2024/11/0207.gif)

## The Power of Open Source

Here, we would like to express our gratitude to the following open-source projects:

- Known: https://known.org.cn/

> This is an open-source enterprise-level development framework meticulously crafted based on Blazor technology. With its outstanding features of low-code, cross-platform, and out-of-the-box usability, it breaks the limitations of traditional development and truly realizes the efficient mode of "write once, run anywhere". Its core value lies in its high efficiency and flexibility, bringing about a brand-new transformation to the software development model. It is like a magic key that helps developers effortlessly open the door to digital transformation, calmly handle various challenges, and fuel business growth, opening a new chapter.

The source code of this website is also open-source:

- Repository: https://github.com/dotnet9/CodeWF

> The Code Workshop is a website developed using.NET 9 Blazor. It features technical articles, introductions to open-source projects, and the use of online tools: Code Workshop - Enlightening with Articles, Empowering with Tools.

## Website Technology

The website is built on the open-source project [KnownCMS](https://gitee.com/known/known-cms) of [Known](https://known.org.cn/):

> KnownCMS is a content management system developed based on Blazor. The front end uses Blazor static components, and the back end uses the Known framework.

Since the website owner's website is mainly for displaying blog articles and using online tools, and usually, Typora and VS Code are used in combination for article editing. The core data files of the website are stored in the [Assets.Dotnet9](https://github.com/dotnet9/Assets.Dotnet9) repository. Therefore, the website owner has removed the back-end management-related functions that are not currently in use. There are only three projects in the project source code:

![](https://img1.dotnet9.com/2024/11/0209.png)

- AntBlazor: The website owner has hardly made any changes to this project. It is basically the encapsulation of Ant Design style Blazor static components provided by Known, such as basic components like forms, labels, buttons, etc.
- CodeWF: A Razor library that mainly encapsulates website documents and blog post pages. Currently, some tools are not yet online. Later, another library will be created according to the architecture of this library to write online tools.
- WebSite: The entry project of the website, integrating the CodeWF and AntBlazor projects. Of course, it also includes the encapsulation of some pages (home page, about page, timeline, etc.), Web API controllers, etc.

| AntBlazor | CodeWF | WebSite |
|---|---|---|
| <img src="https://img1.dotnet9.com/2024/11/0210.png" alt="AntBlazor" style="max-height: 350px;"> | <img src="https://img1.dotnet9.com/2024/11/0211.png" alt="CodeWF" style="max-height: 350px;"> | <img src="https://img1.dotnet9.com/2024/11/0212.png" alt="WebSite" style="max-height: 350px;"> |

**Mini Knowledge: What is Static SSR?**

Static SSR is significantly different from Blazor Server or Blazor Client (WASM). As described in the [Microsoft documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/class-libraries-and-static-server-side-rendering?view=aspnetcore-9.0):

> Static SSR is a unique running mode. When the server processes incoming HTTP requests, components run on the server side. During this process, Blazor will skillfully render the components into HTML and include it in the response content. Once the response is sent, the server-side components and the corresponding renderer state will be automatically discarded, leaving only pure HTML on the browser side.

> This mode has multiple advantages. Firstly, it greatly reduces hosting costs, alleviating the financial burden on website operators. Secondly, it has excellent scalability, being able to handle both small-scale user visits and large-scale traffic surges with ease. This is because it does not require continuous server resources to maintain component states, thus saving a large amount of server resources. Moreover, it breaks free from the constraint of continuous connections between the browser and the server, and there is no need to load WebAssembly in the browser, further optimizing performance.

From a more straightforward perspective, static SSR and Blazor Server both fall into the category of server-side rendering, but they differ in interactive capabilities. In the static SSR mode, the front-end HTML controls cannot use C# event method mappings as in Blazor Server. However, it can still achieve interaction with the help of JS functions. For example, the click event of a button can be mapped to a JS function for processing. Fortunately, important functions such as C# entity binding and service injection can still be used normally in static SSR. This characteristic makes static SSR an ideal choice for front-end websites that require SEO (Search Engine Optimization, which means improving the ranking of a website in search engines through a series of technical means to increase website traffic. The core lies in ensuring that website content can be effectively crawled by search engines to obtain more traffic). The following is a definition of a static SSR component (the basic information component of article details, UPostCount.raozr):

```html
@inject IOptions<SiteOption> SiteOption
<div class="counts">
    @if (Post?.Lastmod!= null)
    {
        <span>Updated on @(Post?.Lastmod?.ToString("yyyy-MM-dd HH:mm:ss"))</span>
    }
    else
    {
        <span>Created on @(Post?.Date?.ToString("yyyy-MM-dd HH:mm:ss"))</span>
    }
    <span style="margin:0 5px;">|</span>
    <span class="author">@(string.IsNullOrWhiteSpace(Post?.Author)? SiteOption.Value.Owner : Post!.Author)</span>
    @if (ShowEdit)
    {
        <span style="margin:0 5px;">|</span>
        <a href="@ConstantUtil.GetPostGithubPath(SiteOption.Value.RemoteAssetsRepository, Post)" target="_blank">I want to edit, leave a comment</a>
    }
</div>

@code {
    [Parameter] public BlogPost Post { get; set; }
    [Parameter] public bool ShowEdit { get; set; } = true;
}
```

The effect is as follows:

![](https://img1.dotnet9.com/2024/11/0208.png)

## Website Function Explanation

### Home Page

- Website URL: https://dotnet9.com

Like most websites, it first displays the website slogan **"Code Workshop: Developed using.NET 9 Web API + Blazor. Features technical articles, introductions to open-source projects, and online tools to facilitate efficient programming."**, then showcases featured article blocks, and later (currently under development) will add featured tool blocks, followed by friendly links, footer, etc.:

![](https://img1.dotnet9.com/2024/11/0202.gif)

### Documentation

Here, some of the website owner's open-source projects are introduced:

- Website URL: https://dotnet9.com/doc

![](https://img1.dotnet9.com/2024/11/0203.gif)

The following are brief introductions to some of the projects:

1. CodeWF

This is the source code repository of this website. You can click [the link](https://github.com/dotnet9/CodeWF) to view it.

2. CodeWF.EventBus

It is applicable for in-process event passing (without other external dependencies), and its function is similar to that of MediatR. You can click [the link](https://github.com/dotnet9/CodeWF.EventBus) to view it.

3. CodeWF.EventBus.Socket

CodeWF.EventBus.Socket is a lightweight, Socket-based distributed event bus system designed to simplify event communication in distributed architectures. It allows processes to communicate through the publish/subscribe pattern without relying on external message queue services. You can click [the link](https://github.com/dotnet9/CodeWF.EventBus.Socket) to view it.

4. CodeWF.NetWeaver

CodeWF.NetWeaver is a concise and powerful C# library that supports AOT and is used for packet assembly and disassembly operations of TCP and UDP data packets. You can click [the link](https://github.com/dotnet9/CodeWF.NetWeaver) to view it.

5. CodeWF.Toolbox

CodeWF.Toolbox is a cross-platform toolbox developed using Avalonia, using Prism as the modular development framework and supporting AOT publication. It can be used for learning Avalonia projects. You can click [the link](https://github.com/dotnet9/CodeWF.Toolbox) to view it.

6. CodeWF.Tools

Here, commonly used tool classes are collected and shared. You can click [the link](https://github.com/dotnet9/CodeWF.Tools) to view it.

7. Assets.Dotnet9

This is the core data repository of this website. You can click [the link](https://github.com/dotnet9/Assets.Dotnet9) to view it.

### Blog

- Website URL: https://dotnet9.com/bbs/

The blog page is in the standard technical blog style, divided into left, middle, and right columns. The left column is the article classification list. Clicking on it allows you to browse the article list in pages in the middle column. The right column contains website statistics, website owner's recommendations, etc.:

![](https://img1.dotnet9.com/2024/11/0204.png)

Clicking on an article in the list allows you to browse the detailed content of the article. Here, we would like to thank [VleaStwo]([VleaStwo (Lee)](https://github.com/VleaStwo)) for providing the TOC function: This function enables users to quickly locate the key content of the article, improving reading efficiency.

![](https://img1.dotnet9.com/2024/11/0205.gif)

**You can modify all articles**

If there are typos, grammar errors, misleading content, or if you have anything to add to an article, you can click on "I want to edit, leave a comment" in the upper right corner of the page header to submit a PR. Thank you very much!

![](https://img1.dotnet9.com/2024/11/0208.png)

The latest [PR#4](https://github.com/dotnet9/Assets.Dotnet9/pull/4) for the article [. NET Cross-Platform Client Framework - Avalonia UI](https://dotnet9.com/bbs/post/2022/11/one-of-the-best-choices-for-dotnet-cross-platform-frameworks-avalonia-ui):

![](https://img1.dotnet9.com/2024/11/0213.png)

Thanks to the netizen [hjkl950217 (Chang Kong X)](https://github.com/hjkl950217)

## Summary

During the development process of the website, the website owner has been constantly exploring and trying, learning a large number of tutorials and open-source projects, which have been very helpful. We are very grateful to the friends and teachers who have provided assistance.

In addition, [VleaStwo](https://github.com/VleaStwo) has opened a [Masa Blazor branch](https://github.com/VleaStwo/CodeWF). Friends who are interested are welcome to submit PRs or communicate:

![](https://img1.dotnet9.com/2024/11/0206.png)

Finally, relevant links are attached for everyone to understand and communicate:

- Source code of this website (Ant Design style): https://github.com/dotnet9/codewf
- [VleaStwo](https://github.com/VleaStwo) branch (Masa Blazor style): https://github.com/VleaStwo/CodeWF
- Known: https://known.org.cn
- Ant Design Blazor: https://antblazor.com/
- Masa Blazor: https://masastack.com/blazor

## OpenAPI

https://localhost:5002/swagger/v1

## Sponsorship

> If you think this project is helpful to you, you can buy the author a cup of coffee to show your encouragement ☕️

| WeChat Pay | Alipay | QQ Pay |
|---|---|---|
|![](https://img1.dotnet9.com/site/pays/WeChatPay.jpg) |![](https://img1.dotnet9.com/site/pays/AliPay.jpg) |![](https://img1.dotnet9.com/site/pays/QQPay.jpg) |

## Thanks

- [KnownCMS](https://gitee.com/known/known-cms)