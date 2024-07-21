# 服务调用 - 概述

## 概念

服务调用是指一个应用程序通过网络请求另一个应用程序提供的服务。通过它可以调用其它服务的接口来获取所需的功能或数据。

## 最佳实践

MASA Framework 的服务调用组件提供了基于 `HttpClient` 和 `Dapr` 服务调用的能力

* [HttpClient](/framework/building-blocks/caller/httpclient)： 基于 [.NET Core 的 HttpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient) 实现的服务调用
* [DaprClient](/framework/building-blocks/caller/daprclient)：基于 `DaprClient` 实现的服务调用

## 源码解读

当返回类型为
* `TResponse`：`自定义返回类型`，框架自行处理异常请求
* `其它类型 (非自定义返回类型)`：根据传入参数 `autoThrowException` 的值决定是否默认处理框架异常，默认：true

框架处理异常请求机制，当请求响应的 `HttpStatusCode` 为
* `299`：上抛 `UserFriendlyException` 异常
* `298`：上抛 `ValidatorException` 异常

### ICaller

服务调用抽象，它提供了以下能力

> `autoThrowException` 为 `true` 会检查 `HttpStatus` 状态码并抛出对应的 `Exception`，部分方法的返回类型是指定类型，且没有 `autoThrowException` 参数，那么它们会自动检查 `HttpStatus` 状态码并抛出对应的 `Exception`（gRPC 请求除外）

* SendAsync：提供原始的 `Send` 方法，需要自行提供 `HttpRequestMessage` 类型的请求信息
* SendGrpcAsync：提供基于 `gRPC` 的请求
* GetStringAsync：提供 `Get` 请求并获取返回类型为 `string` 的结果
* GetByteArrayAsync：提供 `Get` 请求并获取返回类型为 `byte[]` 的结果
* GetStreamAsync：提供 `Get` 请求并获取返回类型为 `Stream` 的结果
* GetAsync：提供 `Get` 请求并获取返回类型为 `指定类型` 的结果
* PostAsync：提供 `Post` 请求并获取返回类型为 `指定类型` 的结果
* PatchAsync：提供 `Patch` 请求并获取返回类型为 `指定类型` 的结果
* PutAsync：提供 `Put` 请求并获取返回类型为 `指定类型` 的结果
* DeleteAsync：提供 `Delete` 请求并获取返回类型为 `指定类型` 的结果

### ICallerFactory

服务调用抽象工厂，它提供了以下能力

* Create：创建提供者

> 当不存在 `name` 为 `string.Empty` 的提供者时，从 `Caller` 提供者列表中取第一个

### IRequestMessage

请求消息抽象，提供了处理 `HttpRequestMessage` 的请求消息抽象，默认实现：[`JsonRequestMessage`](https://github.com/masastack/MASA.Framework/blob/main/src/BuildingBlocks/Service/Masa.BuildingBlocks.Service.Caller/Infrastructure/Json/JsonRequestMessage.cs)

* ProcessHttpRequestMessageAsync：处理请求消息默认程序

### IResponseMessage

响应消息抽象，提供了处理 `HttpResponseMessage` 的响应消息抽象，默认实现：[`JsonResponseMessage`](https://github.com/masastack/MASA.Framework/blob/main/src/BuildingBlocks/Service/Masa.BuildingBlocks.Service.Caller/Infrastructure/Json/JsonResponseMessage.cs)

* ProcessResponseAsync：针对指定响应类型的处理程序
