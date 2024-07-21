---
title: .NET 9 X86 AOT的突破 - 支持老旧Win7与XP环境
slug: good-news-breakthrough-in-net-9-x86-aot-supports-old-win7-and-xp-environments
description: .NET 9开始，X86 AOT支持Win7和XP，不仅仅只支持SP1版本
date: 2024-07-16 05:24:47
lastmod: 2024-07-18 20:51:36
copyright: Original
draft: false
cover: https://img1.dotnet9.com/2024/07/cover_01.png
categories: .NET
tags: .NET 9,AOT,X86,Win7,XP,SP1
---

![](https://img1.dotnet9.com/2024/07/0701.png)

![](https://img1.dotnet9.com/2024/07/0709.png)

## 引言

随着技术的不断进步，微软的.NET 框架在每次迭代中都带来了令人惊喜的新特性。在.NET 9 版本中，一个特别引人注目的亮点是 X86 架构下的 AOT（ Ahead-of-Time）编译器的支持扩展，它允许开发人员将应用程序在编译阶段就优化为能够在老旧的 Windows 系统上运行，包括 Windows 7 和甚至 Windows XP。这不仅提升了性能，也为那些依然依赖这些老平台的企业和个人开发者提供了新的可能性。

**小知识普及：**

1. NET 9 X86 AOT 简介

.NET 9 的 X86 AOT 编译器通过静态编译，将.NET 应用程序转换为可以直接在目标机器上执行的可执行文件，消除了在运行时的 JIT（Just-In-Time）编译所需的时间和资源。这对于对性能要求高且需要支持旧版系统的场景具有显著优势。

2. 支持 Windows 7 与 Windows XP 的背景

尽管 Windows 7 和 XP 已经不再是主流操作系统，但它们在某些特定领域，如企业遗留系统、嵌入式设备或者资源受限的环境中仍有广泛应用。.NET 9 的 AOT 编译器的这一扩展，旨在满足这些场景的兼容性和性能需求。

3. 如何实现

- **编译过程优化**：NET 9 在 AOT 编译时，对代码进行了更为细致的优化，使得生成的可执行文件更小，启动速度更快。
- **向下兼容性**：通过精心设计的编译策略，确保了对 Win7 及 XP API 的兼容性，使代码能够无缝运行。
- **安全性考量**：虽然支持老旧系统，但.NET 9 依然注重安全，提供了一定程度的保护机制以抵御潜在的风险。

4. 实例应用与优势

- **性能提升**：AOT 编译后的程序通常比 JIT 执行的程序更快，尤其对于 CPU 密集型任务。
- **部署简易**：无需用户安装.NET 运行时，简化了部署流程。
- **维护成本降低**：对于依赖老旧系统的企业，避免了频繁升级运行时的困扰。

本文只在分享网友实践的一个成果，如有更多发现，欢迎投稿。

## Windows 7 支持

下图是网友编译的 Avalonia UI 跨平台项目在 Win 7 非 SP1 环境运行效果截图：

![](https://img1.dotnet9.com/2024/07/0702.png)

左侧是程序运行界面，右侧是操作系统版本。

![](https://img1.dotnet9.com/2024/07/0705.png)

![](https://img1.dotnet9.com/2024/07/0706.png)

Winform 都可以 x86 aot 运行..

![](https://img1.dotnet9.com/2024/07/0707.png)

Winform 工程配置如下：

![](https://img1.dotnet9.com/2024/07/0710.png)

## Windows XP 支持

目前测试可运行控制台程序：

![](https://img1.dotnet9.com/2024/07/0703.png)

网友得出结论：

![](https://img1.dotnet9.com/2024/07/0711.png)

XP 需要链接 YY-Thunks，参考链接：https://github.com/Chuyu-Team/YY-Thunks

![](https://img1.dotnet9.com/2024/07/0704.png)

大家可关注 YY-Thunks 这个 ISSUE：https://github.com/Chuyu-Team/YY-Thunks/issues/66

![](https://img1.dotnet9.com/2024/07/0708.png)

控制台支持 XP 的工程配置如下：

![](https://img1.dotnet9.com/2024/07/0712.jpg)

网友心得：

![](https://img1.dotnet9.com/2024/07/0713.png)

## 有待加强的部分

经测试Prism框架使用会报错：

![](https://img1.dotnet9.com/2024/07/0714.png)

使用HttpClient也会出错：

![](https://img1.dotnet9.com/2024/07/0715.jpg)

每个公司的不同项目都是极其不同、复杂的，实际发布还需要不断测试，为了支持Windows7、Windows XP可能不得不做出使用库替换、部分API使用取舍等操作，欢迎读者在使用过程中的心得体会进行分享。

## 结语

.NET 9 的 X86 AOT 支持无疑拓宽了.NET 生态的应用范围，为那些需要在老旧平台上运行高性能应用的开发者提供了强大的工具。随着技术的发展，我们期待未来更多的.NET 版本能够进一步打破界限，让编程变得更加灵活和高效。

感谢网友`GSD`及`M$達`分享的这个好消息，大石头这篇文章《各版本操作系统对.NET 支持情况》推荐大家阅读：https://newlifex.com/tech/os_net

**技术交流**

软件开发技术交流添加 QQ 群：771992300

或扫站长微信(`codewf`，备注`加群`)加入微信技术交流群：

![](https://img1.dotnet9.com/site/wechatowner.jpg)
