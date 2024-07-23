---
title: WPF开源控件库 - HandyControl
slug: open-source-wpf-control-library-handycontrol
description: 一个很多人参与贡献的WPF开源控件项目
banner: true
date: 2019-12-09 13:45:56
author: 沙漠尽头的狼
draft: false
cover: https://img1.dotnet9.com/2019/12/0206.png
tags: 开源WPF
categories: .NET
copyright: Original
---

## 前言

历经 3 个白天 2 个黑夜（至凌晨 2 点），Dotnet9 小编经过反复修改、润色，终于完成此文编写（本文略长，手机党请考虑流量），只能说小编我不容易呀不容易。

完成此文编写后，小编我能想象到《HandyControl》控件库作者及众多贡献者们，当初没日没夜码砖编写此控件库的各种研究、容错的场景，他们是一群多么负有激情、多么乐于分享的一群人啊，谢谢你们分享这么一套优秀的控件库给 WPF 从业者。

由于本文略长，建议读者查看以下导航目录，根据读者个人关注点点击阅读，也可按住 Ctrl + F 组合键搜索常用控件名字进行搜索阅读，当然小编是希望读者都能按文章顺序阅读啦，哈哈。

## 一、写在文章最前面的话

应博客园园友 @郭达·斯坦森 推荐，Dotnet9 小编本文介绍开源 C# WPF 控件库《HandyControl》，希望大家能够喜欢，同时亦欢迎大家推荐优秀开源 WPF 控件库给小编，小编在此谢谢大家对 dotnet 技术的关注和支持。

评论在此文第 51 楼：《《Dotnet9》系列-开源 C# WPF 控件库 2《Panuon.UI.Silver》强力推荐》

![](https://img1.dotnet9.com/2019/12/0201.png)

说点本文之前两篇控件库推荐文章的影响：

继前两篇开源 C# WPF 控件库（库 1，库 2）受广大网友推荐后，Dotnet9 小编备受鼓舞，让小编仿佛看到了 dotnet 蓬勃发展的 200 几年。

谢谢大家在博客园的大力推荐和留下的数十条文末评论，使小编我坚定了继续写优质 C# WPF 分享文章的信念，下面是近期博客园首页文章推荐截图：博客园。

![](https://img1.dotnet9.com/2019/12/0202.png)

本站单日 IP 访问量又突破新高，达到了 500 访问量，又上一个新台阶，谢谢广大网友。

![](https://img1.dotnet9.com/2019/12/0203.png)

另外，亦是由于两篇文章大火，Dotnet9 小编的个人博客站点出了点小插曲，以下是本站最新快讯：

![](https://img1.dotnet9.com/2019/12/0204.png)

但本站不会因该小插曲而停止继续给大家分享优质文章的步伐，以上是站长的声明，谢谢大家继续支持本站站长 Dotnet9 小编。

## 二、关于控件库《HandyControl》

### 2.1 交流社区

github 地址：[https://github.com/HandyOrg/HandyControl](https://github.com/HandyOrg/HandyControl) 。

贡献者：NaBian、yanchao891012、ghost1372、guanguanchuangyu、noctwolf、DingpingZhang、xianyun666、M0n7y5、gitter-badger、afunc233 等等。

作者推荐的 C#及 WPF 学习博客链接：纳边、林德熙、吕毅、DinoChan、玩命夜狼 等等。

以下是两种主题控件库概览，先给大家一个大致印象，然后我开始介绍该控件库经典案例及详细控件介绍，希望大家喜欢我这样的介绍风格。

### 2.2 白色主题

![](https://img1.dotnet9.com/2019/12/0205.png)

### 2.3 黑色主题

![](https://img1.dotnet9.com/2019/12/0206.png)

## 三、基于控件库衍生的经典 Case 案例

优秀的控件库肯定就有一群志同道合的小伙伴追随，从控件库作者建立的两个 QQ 群人数即可看出，使用此控件库的朋友很多，Dotnet9 小编就和控件库作者从中遴选出几个比较典型的项目举例，读者朋友可以看看，《HandyControl》控件库是不是非常适合您的项目？

### 3.1 Case 案例 1

软件名：phpEnv，浏览地址：https://www.phpenv.cn/ 。

软件简介：phpEnv 是运行在 Windows 系统上的完全绿色的 PHP 集成环境，集成了 Apache、Nginx 等 Web 组件，支持不同 PHP 版本共存，支持自定义 PHP 版本，自定义 MySQL 版本。主打开发环境，也可以用作服务器环境。拥有清除 PHP 环境阻碍、解除端口占用、支持切换 MySQL 版本、修改 MySQL 密码，兼容其他集成环境，内置 Redis、MemCache 等其它服务，内置 Composer 和功能强大的 CMD 命令行、TCP 端口进程列表等工具和实用功能。

### 3.2 Case 案例 2

软件名：AutomnBox，浏览地址：https://github.com/zsh2401/AutumnBox 。

AutumnBox 是什么?一个对 Google Adb 工具包进行 GUI 封装的桌面程序,方便小白,帮助老鸟。

AutumnBox 能干什么?

1. 为您的设备刷入第三方 Recovery
2. 向设备推送文件
3. 一键激活黑域服务
4. 一键激活冰箱
5. 解锁 System,获取完整 root 控制权
6. 以拓展模块为中心的功能开发思想,将来将会支持越来越多的功能
7. …

## 四、特色控件详细介绍

介绍控件肯定少不了特色控件截图和文字描述，编写本文时，Dotnet9 小编不用再自己截图、录制 gif 动画了等素材了，因为 《HandyControl》控件库作者非常优秀，本文大部分图片素材来自控件库作者 github 仓库，读者您可以直接访问此地址查看：https://github.com/HandyOrg/HandyControl 。

下面 Dotnet9 小编介绍 HC（后文作者使用此简写表示 HandyControl）控件时，会加上自己的使用体验及观点，如有不同观点或建议，请在文末留言和小编讨论，或者加作者 QQ 交流群切磋交流，大家以技术会友，共同成长。

### 4.1 各式按钮

界面开发首先想到的就是按钮，下面是《HandyControl》设计的几类按钮，是否有您中意的一款？

#### 4.1.1 普通按钮(Button)

普通按钮(Button)，一般桌面开发中，以下样式的按钮应该已经够用了，当然也可以根据自家公司设计师的要求，在作者样式基础上加以扩展修改也是极方便的。

![](https://img1.dotnet9.com/2019/12/0207.png)
