# 码坊

**码坊—文章启智，工具助力**

简体中文 | [English](README.md)

## 介绍

码坊是使用.NET 10 Blazor开发的一个网站，这里有技术文章，有开源项目介绍，有在线工具使用:  码坊—文章启智，工具助力

在博客网站的开发征程中，站长可谓是一路披荆斩棘。从最初的构思到实践，先后涉足了多种开发技术，包括 [MVC]([ASP.NET Core MVC 概述 | Microsoft Learn](https://learn.microsoft.com/zh-cn/aspnet/core/mvc/overview?view=aspnetcore-9.0))、[Razor Pages]([ASP.NET Core 中的 Razor Pages 介绍 | Microsoft Learn](https://learn.microsoft.com/zh-cn/aspnet/core/razor-pages/?view=aspnetcore-9.0&tabs=visual-studio))、[Vue]([Vue.js - 渐进式 JavaScript 框架 | Vue.js (vuejs.org)](https://cn.vuejs.org/))、[Go]([The Go Programming Language (google.cn)](https://golang.google.cn/))、[Blazor]([ASP.NET Core Blazor | Microsoft Learn](https://learn.microsoft.com/zh-cn/aspnet/core/blazor/?view=aspnetcore-9.0)) 等。在这漫长的过程中，网站版本更迭近 10 次，每一个版本都凝聚着站长的心血与探索，这段充满挑战的历程详细记录于 [分享我做Dotnet9博客网站时积累的一些资料 - 码坊](https://dotnet9.com/bbs/post/2022/3/Share-some-learning-materials-I-accumulated-when-I-was-a-blog-website)。

如今，经过深思熟虑与实践检验，博客网站再次回归 Blazor，并采用了静态 SSR 技术，同时融入了时尚且实用的 Ant Design 设计风格。这一系列的改进使得网站的访问速度得到了质的飞跃，如同给网站注入了新的活力，目前网站已经成功上线。

- 网址：https://dotnet9.com

![](https://img1.dotnet9.com/2024/11/0207.gif)

## 开源的力量

在此，要感谢以下开源项目：

- Known: https://known.org.cn/

>这是一个开源企业级开发框架，基于 Blazor 技术精心打造。它以低代码、跨平台、开箱即用的卓越特性，打破了传统开发的局限，真正实现了一处代码，多处运行的高效模式。其核心价值在于高效与灵活，为软件开发模式带来了全新的变革，就像一把神奇的钥匙，帮助开发者轻松开启数字化转型的大门，从容应对各种挑战，助力业务实现蓬勃发展，开启崭新篇章。

本站源码也是开源：

- 仓库：https://github.com/dotnet9/CodeWF

> 码坊是使用.NET 10 Blazor开发的一个网站，这里有技术文章，有开源项目介绍，有在线工具使用: 码坊—文章启智，工具助力

## 网站技术

网站是基于 [Known](https://known.org.cn/) 的开源项目 [KnownCMS](https://gitee.com/known/known-cms) 搭建：

> KnownCMS是基于Blazor开发的一个内容管理系统，前台使用Blazor静态组件，后台使用Known框架。

因为站长的网站只是一个博客文章展示、在线工具使用，平时文章编辑也是使用Typora及VS Code搭配使用，网站核心数据文件存储于 [Assets.Dotnet9](https://github.com/dotnet9/Assets.Dotnet9) 仓库，所以站长去除了暂时不使用的后台管理相关功能，项目源码只有3个工程：

![](https://img1.dotnet9.com/2024/11/0209.png)

- AntBlazor：站长基本没有改过该工程，基本是由Known提供的Ant Design风格Blazor静态组件封装，比如表单、标签、按钮之类的基本组件等。
- CodeWF：Razor类库，主要实现网站文档、博文页面封装，目前有工具还未上线，后面会按此库架构另开一个库写在线工具；
- WebSite：网站的入口工程，整合CodeWF和AntBlazor工程，当然也包括部分页面封装（首页、关于、时间线等）、Web API控制器等

<table>
    <tr>
    	<td>AntBlazor</td>
        <td>CodeWF</td>
        <td>WebSite</td>
    </tr>
    <tr>
        <td><img src="https://img1.dotnet9.com/2024/11/0210.png" alt="AntBlazor" style="max-height: 350px;"></td>
        <td><img src="https://img1.dotnet9.com/2024/11/0211.png" alt="CodeWF" style="max-height: 350px;"></td>
        <td><img src="https://img1.dotnet9.com/2024/11/0212.png" alt="WebSite" style="max-height: 350px;"></td>
    </tr>
</table>

**小知识：什么是静态 SSR？**

静态 SSR 与 Blazor Server 或 Blazor Client（WASM）有着显著的区别，[微软文档](https://learn.microsoft.com/zh-cn/aspnet/core/blazor/components/class-libraries-and-static-server-side-rendering?view=aspnetcore-9.0) 的说明：

>静态 SSR 是一种独特的运行模式，在服务器处理传入 HTTP 请求时，组件在服务器端运行。在此过程中，Blazor 会将组件巧妙地呈现为 HTML，并将其包含在响应内容之中。当响应发送完成后，服务器端组件和相应的呈现器状态会被自动丢弃，最终在浏览器端仅留存纯净的 HTML。

>这种模式的优势是多方面的。首先，它极大地降低了托管成本，为网站运营者减轻了经济负担。其次，它具有出色的可缩放性，无论是面对小规模的用户访问，还是大规模的流量冲击，都能应对自如。这得益于它无需持续的服务器资源来维持组件状态，从而节省了大量服务器资源。而且，它摆脱了浏览器和服务器之间持续连接的束缚，同时也无需在浏览器中加载 WebAssembly，进一步优化了性能。

从更通俗易懂的角度来看，静态 SSR 与 Blazor Server 同属服务端渲染的范畴，但它在交互能力方面有所不同。在静态 SSR 模式下，前端的 HTML 控件不能像在 Blazor Server 中那样使用 C# 事件方法映射，不过它仍然可以借助 JS 函数来实现交互，例如 button 的 click 事件可以映射 JS 函数进行处理。值得庆幸的是，C# 实体绑定、服务注入等重要功能在静态 SSR 中依然可以正常使用。这一特性使得静态 SSR 成为需要 SEO（搜索引擎优化，即通过一系列技术手段提升网站在搜索引擎中的排名，进而增加网站流量。其核心在于确保网站内容能够被搜索引擎有效抓取，从而获得更多流量）的前台网站的理想选择。以下是一个静态 SSR 组件定义（文章详情基本信息组件 UPostCount.raozr）：

```html
@inject IOptions<SiteOption> SiteOption
<div class="counts">
    @if (Post?.Lastmod != null)
    {
        <span>更新于@(Post?.Lastmod?.ToString("yyyy-MM-dd HH:mm:ss"))</span>
    }
    else
    {
        <span>创建于@(Post?.Date?.ToString("yyyy-MM-dd HH:mm:ss"))</span>
    }
    <span style="margin:0 5px;">|</span>
    <span class="author">@(string.IsNullOrWhiteSpace(Post?.Author) ? SiteOption.Value.Owner : Post!.Author)</span>
    @if (ShowEdit)
    {
        <span style="margin:0 5px;">|</span>
        <a href="@ConstantUtil.GetPostGithubPath(SiteOption.Value.RemoteAssetsRepository, Post)" target="_blank">我要编辑、留言</a>
    }
</div>

@code {
    [Parameter] public BlogPost Post { get; set; }
    [Parameter] public bool ShowEdit { get; set; } = true;
}
```

效果如下：

![](https://img1.dotnet9.com/2024/11/0208.png)

## 网站功能说明

### 首页

- 网址：https://dotnet9.com

和大多数网站一样，先展示网站宣传语 **”码坊：使用.NET 10 Web API + Blazor开发。有技术文章、开源项目介绍和在线工具，助力高效编程。“**，然后展示特色文章块，后面会加上特色工具块（正在开发中），最后是友情链接、页尾等：

![](https://img1.dotnet9.com/2024/11/0202.gif)

### 文档

这里介绍了站长部分开源项目：

- 网址：https://dotnet9.com/doc

![](https://img1.dotnet9.com/2024/11/0203.gif)

下面是部分项目简介

1. CodeWF

这是本站的源码仓库，可点击[链接](https://github.com/dotnet9/CodeWF)查看。

2. CodeWF.EventBus

它适用于进程内事件传递（无其他外部依赖），功能与 MediatR 类似，可点击[链接](https://github.com/dotnet9/CodeWF.EventBus)查看。

3. CodeWF.EventBus.Socket

CodeWF.EventBus.Socket 是一个轻量级、基于 Socket 的分布式事件总线系统，旨在简化分布式架构中的事件通信。它允许进程之间通过发布 / 订阅模式进行通信，无需依赖外部消息队列服务。可点击[链接](https://github.com/dotnet9/CodeWF.EventBus.Socket)查看。

4. CodeWF.NetWeaver

CodeWF.NetWeaver 是一个简洁而强大的C#库，支持AOT，用于处理TCP和UDP数据包的组包和解包操作。可点击[链接](https://github.com/dotnet9/CodeWF.NetWeaver)查看。

5. CodeWF.Toolbox

CodeWF Toolbox 使用Avalonia开发的跨平台工具箱，使用了Prism做为模块化开发框架，支持AOT发布，可做为Avalonia项目学习。可点击[链接](https://github.com/dotnet9/CodeWF.Toolbox)查看。

6. CodeWF.Tools

这里收集、分享了常用工具类，可点击[链接](https://github.com/dotnet9/CodeWF.Tools)查看。

7. Assets.Dotnet9

这是本站的核心数据仓库，可点击[链接](https://github.com/dotnet9/Assets.Dotnet9)查看。

### 博客

- 网址：https://dotnet9.com/bbs/

博客页面是标准的技术博客风格样式，分为左、中、右三栏。左边栏是文章分类列表，点击可在中间分页浏览文章列表，右侧则是网站统计、站长推荐等内容：

![](https://img1.dotnet9.com/2024/11/0204.png)

点击列表中的文章可浏览文章详细内容，在此要感谢[VleaStwo]([VleaStwo (Lee)](https://github.com/VleaStwo))大佬提供的TOC功能：这个功能使用户能够快速定位文章的重点内容，提高阅读效率。

![](https://img1.dotnet9.com/2024/11/0205.gif)

**所有文章您都可以修改**

如果文章有错别字、语病，或有误导的地方，或您有什么补充，可点击页头右上角“我要编辑、留言”进行PR，十分感谢！

![](https://img1.dotnet9.com/2024/11/0208.png)

最新一个对文章  [. NET 跨平台客户端框架 - Avalonia UI](https://dotnet9.com/bbs/post/2022/11/one-of-the-best-choices-for-dotnet-cross-platform-frameworks-avalonia-ui) 的 [PR#4](https://github.com/dotnet9/Assets.Dotnet9/pull/4)：

![](https://img1.dotnet9.com/2024/11/0213.png)

感谢网友 [hjkl950217 (长空X)](https://github.com/hjkl950217)

## 总结

在网站的开发历程中，站长不断探索尝试，学习了大量的教程和开源项目，非常受用、非常感谢提供帮助的朋友和老师。

另外，[VleaStwo](https://github.com/VleaStwo)大佬开了一个 [Masa Blazor分支](https://github.com/VleaStwo/CodeWF)，欢迎有兴趣的朋友前来 PR 或交流：

![](https://img1.dotnet9.com/2024/11/0206.png)

最后，贴上相关链接，大家可以了解、交流：

- 本站源码(Ant Design风格)：https://github.com/dotnet9/codewf
- [VleaStwo](https://github.com/VleaStwo)大佬分支(Masa Blazor风格)：https://github.com/VleaStwo/CodeWF
- Known：https://known.org.cn
- Ant Design Blazor：https://antblazor.com/
- Masa Blazor：https://masastack.com/blazor

## OpenAPI

https://localhost:5002/scalar/v1

## 赞助

> 如果你觉得这个项目对你有帮助，你可以请作者喝杯咖啡表示鼓励 ☕️

| 微信支付                                              | 支付宝                                             | QQ支付                                            |
| ----------------------------------------------------- | -------------------------------------------------- | ------------------------------------------------- |
| ![](https://img1.dotnet9.com/site/pays/WeChatPay.jpg) | ![](https://img1.dotnet9.com/site/pays/AliPay.jpg) | ![](https://img1.dotnet9.com/site/pays/QQPay.jpg) |



## 感谢

- [KnownCMS](https://gitee.com/known/known-cms)