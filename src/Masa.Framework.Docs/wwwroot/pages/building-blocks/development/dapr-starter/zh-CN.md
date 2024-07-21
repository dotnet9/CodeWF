# 开发 - DaprStarter

## 概述

为了方便在开发过程中使用 `Dapr`，而不需要部署 `Kubernetes`、`Docker Compose` 环境，更不需要通过手动启动 `dapr sidecar`，提供了 `DaprStarter` 协助管理 `dapr sidecar`

在 MASA Framework 中提供了以下包:

* [`Masa.Contrib.Development.DaprStarter`](https://nuget.org/packages/Masa.Contrib.Development.DaprStarter)：`Dapr Starter` 核心功能库，包含 `dapr sidecar` 的启动、停止等操作
* [`Masa.Contrib.Development.DaprStarter.AspNetCore`](https://nuget.org/packages/Masa.Contrib.Development.DaprStarter.AspNetCore): 为 `Asp.Net Core` 的 Web 程序提供一站式方案，项目启动时会自动启动 `dapr sidecar`



## 使用

1. [安装 Dapr-Cli](https://docs.dapr.io/zh-hans/getting-started/install-dapr-cli/) 并初始化 [Dapr](https://docs.dapr.io/zh-hans/getting-started/install-dapr-selfhost/)

2. 安装  `Masa.Contrib.Development.DaprStarter.AspNetCore`

   ```shell 终端
   dotnet add package Masa.Contrib.Development.DaprStarter.AspNetCore
   ```

3. 启动 `DaprStarter`

   ```csharp Program.cs l:2
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddDaprStarter();
   ```

## 高阶用法

### 启动DaprStarter

`Dapr Starter` 启动可支持配置，对于未配置的必须参数会根据约定自动生成

#### 默认装配

```csharp Program.cs l:2
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprStarter();
```

#### 代码指定配置 + 约定

```csharp Program.cs l:2-6
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprStarter(opt =>
{
    opt.AppPort = 5001;
    opt.DaprHttpPort = 8080;
    opt.DaprGrpcPort = 8081;
});
```

#### 配置文件 + 约定

:::: code-group
::: code-group-item appsettings.json

```json appsettings.json l:2-8
{
  "DaprOptions": {
    "AppId": "masa-dapr-test",
    "AppPort": 5001,
    "AppIdSuffix": "",
    "DaprHttpPort": 8080,
    "DaprGrpcPort": 8081
  }
}
```

:::
::: code-group-item 注册DaprStarter

```csharp Program.cs l:2
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprStarter();
```

:::
::::

> 优势：更改 `appsettings.json` 配置后，`dapr sidecar` 会自动更新，项目无需重启

除上述三种方法以外，还支持 [`选项模式`](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/options)，Dapr Starter的数据源是 `IOptionsMonitor<DaprOptions>`，借助 [`MasaConfiguration`](/framework/building-blocks/configuration)，我们也可以将 Dapr Starter的配置存储到远程配置中心，但这个是没有太大必要的，它只是开发环境下用来运行 dapr sidecar 的

## 配置

### 优先级

**代码指定 > 按规则生成**

### 规则

为更方便我们在项目中使用 `Dapr Sidecar`，我们约定：

1. `dapr appid` 生成规则：`AppId + AppIdDelimiter + AppIdSuffix`
   
    1. AppId: 项目名.Replace(".", "-")
    2. AppIdDelimiter：-
    3. AppIdSuffix：当前机器网卡地址 (默认)
    
    > 指定 `AppId` -> 全局 `AppId` -> 按照约定生成 `AppId`
    
2. `AppPort`: 自动获取项目启动的端口

3. `DaprHttpPort`: 如果未通过代码指定、配置文件指定方式，且环境变量中也未指定 `DAPR_HTTP_PORT` 的值，则由 `dapr run` 自动分配未使用的端口作为 `Dapr Sidecar` 的 `HTTP` 端口

4. `DaprGrpcPort`: 如果未通过代码指定、配置文件指定方式，且环境变量中也未指定 `DAPR_GRPC_PORT` 的值，由 `dapr run` 自动分配未使用的端口作为 `Dapr Sidecar` 的 `gRPC` 端口

## 源码解读

* IAppPortProvider: 获取应用程序端口接口
* DaprBackgroundService: 后台任务处理程序，用于项目启动后协助启动 `dapr sidecar`，以及当服务停止后，停止 `dapr sidecar`
* IDaprProvider: `dapr` 程序接口，提供获取目前可用的 `dapr` 程序列表，启动 `dapr`、停止 `dapr`、判断指定 `appid` 是否启动
* IDaprEnvironmentProvider: `dapr` 环境接口提供者，提供获取 `dapr` 使用的 `GrpcPort`、`HttpPort` 信息以及设置 `GrpcPort`、`HttpPort` 等功能
* DaprProcess: Dapr程序实现，提供 `dapr sidecar`的启动、停止以及心跳检测dapr是否存活、触发补全dapr所需环境变量的功能

## 知识扩展

我们知道，使用 `dapr sidecar` 最少需要2个参数，它们分别是:

* `appid`: `dapr appid`
* `app-port`: 应用程序的端口

我们在使用时，最为方便的是使用默认装配，无需配置任何参数，项目启动时会自动启动 `Dapr Sidecar`。但其存在劣势：

默认装配不会在项目启动时立即启动 `Dapr Sidecar`，会延迟启动，因此使用默认装配启动的应用程序，请不要在后台任务中直接使用 `DaprClient`，或者改为延迟获取 `DaprClient` ，确保获取 `DaprClient` 在 `Dapr Sidecar` 启动之后（`DaprStarter` 会协助补全 `DaprHttpPort`、`DaprGrpcPort` 端口信息，如果获取 `DaprClient` 实例在 `DaprStarter` 启动之前，那造成的结果是获取到错误的端口配置，`HttpPort`: 3500，`GRpcPort`: 50001。 最后导致使用 `dapr` 的功能出错）

如果需要在后台任务中使用 `DaprClient`，我们建议不要使用 **延迟启动 **`Dapr Sidecar`，并至少为 `AppPort` 赋值，确保开发的正常运行。

```
builder.Services.AddDaprStarter(opt =>
{
    opt.AppPort = 5001;
}, false);
```