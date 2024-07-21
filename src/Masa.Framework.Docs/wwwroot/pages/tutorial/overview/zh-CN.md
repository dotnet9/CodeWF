# 实战教程 - 概述

## 概述

在本系列教程中，我们会通过预设一个场景，帮助大家去了解 `MASA Framework`

* 场景：提供一个后端服务，支持**产品的进行增删改查**
* 基本要求：
  * 支持查询已删除的产品
  * 创建产品后需要通知仓库管理人
* 目的：
  * 为了让大家了解更多`MASA Framework`的能力 （教程将从简单→复杂，一步步重构到最终的结构）
* 技术栈：
  * MinimalAPIs （最小API）：提供API服务
  * MasaDbContext （数据上下文）：提供数据增删改查
  * EventBus：事件总线
  * CQRS：读写分离
  * Caching: 缓存
  * DDD：领域驱动设计

  <!-- 注释内容，暂不展示，请勿删除 -->
  <!-- * MasaConfiguration：配置（提供强类型的配置，支持监听配置更新）---->

> 友情提示：按需使用 [`Building Block`](/framework/concepts/building-blocks) 有助于更高效的完成工作

## 目录

1. [服务端](/framework/tutorial/mf-part-1)
2. [数据上下文](/framework/tutorial/mf-part-2)
3. [事件总线和读写分离](/framework/tutorial/mf-part-3)
4. [全局异常](/framework/tutorial/mf-part-4)
5. [缓存](/framework/tutorial/mf-part-5)
6. [领域驱动设计](/framework/tutorial/mf-part-6)

> 持续完善中……

## 下载源码

* [MASA.Framework.Tutorial](https://github.com/masalabs/MASA.Framework.Tutorial)

## 必要条件

1. 确保已安装[`.NET 6.0`](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)以上版本

   ```shell 终端
   dotnet --list-sdks
   ```

   > 通过以上命令验证确保已安装符合条件版本的`.NET SDK`, `MASA Framewokr`最低版本：`.NET6.0`

2. 确保已安装支持 [`.NET 6.0`](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)或支持更高 `.NET SDK` 版本的开发工具，例如：
   * [Visual Studio](https://visualstudio.microsoft.com/zh-hans/downloads/): Visual Studio 2022及更高版本
   * [Rider](https://www.jetbrains.com/rider/): Rider 2022.2及更高版本
   * [Visual Studio Code](https://code.visualstudio.com/download)

## 创建解决方案

可通过以下两种方式创建项目:

### 通过模版创建

1. 安装模版

   ```shell 终端
   dotnet new install MASA.Template
   ```

2. 使用模版创建

   ```shell 终端
   dotnet new masafx -n MASA.EShop --web None
   ```

   > 更多模版命令可通过 **dotnet new masafx -h** ，或者通过Visual Studio图形化界面进行创建

### 手动创建

从零开始，不使用模版

   ```shell 终端
   dotnet new sln -n EShop-MinimalAPIs-Blazor
   ```

## 选择解决方案

在本系列教程中，我们将采用手动方式创建一个解决方案，以完成对产品的增删改查

> 示例项目会把常用的功能以及写法在示例中进行讲解，更多的用法可查看对应的 [**Building Block**](/framework/concepts/building-blocks) 文档

## 常见问题

1. 按照文档操作, 并没有发现对应的方法、属性或类 
   
    可通过尝试安装与[示例](https://github.com/masalabs/MASA.Framework.Tutorial/blob/main/Directory.Build.props)一致的`MASA Framework`版本的预览版再进行重试，如果仍未找到对应的方法、属性或者类，可以尝试在[这里](https://github.com/masastack/MASA.Framework/issues?q=)进行搜索查询，如果仍然未找到答案可以给我们提[Issues](https://github.com/masastack/MASA.Templates/issues/new/choose)

2. 教程中使用的版本号均为`$(MasaFrameworkPackageVersion)`，这个是什么?

    全局配置文件中的版本，查看[文档](/framework/contribution/recommend#section-4f7f75287edf4e007248672c76845305)