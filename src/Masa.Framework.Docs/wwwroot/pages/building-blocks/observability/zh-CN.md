## 概述

我们可观测性基于[OpenTelemetry](https://opentelemetry.io/docs/)的接入，使用标准的OTLPExporter，[Logs](https://opentelemetry.io/docs/concepts/observability-primer/#logs)和[Traces](https://opentelemetry.io/docs/concepts/observability-primer/#distributed-traces)的持久化采用[Elasticsearch](https://www.elastic.co/cn/elasticsearch/)，[Metrics](https://opentelemetry.io/docs/concepts/observability-primer/#reliability--metrics)持久化采用[Prometheus](https://prometheus.io/)。

### Metrics
当前只集成了`OpenTelemetry`，采集的`Metrics`范围较少，可以根据需求，添加第三方更为成熟的`Metrics`监测库，或自定义添加，可参考[Metrics](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics)

### Traces
当前主要集成了`HTTP`和`Database`(EF Core模式)链路追踪，链路相关详细资料可参见[Distributed tracing](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing)和[Collect a distributed trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-collection-walkthroughs?source=recommendations)。

#### Logs
通过集成`OpenTelemetry`后，直接使用[ILogger](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line)记录日志，由于`Logs`记录了相关联的链路`TraceId`，在通过相关日志排查问题时，可以根据链路`TraceId`查找相关的链路信息，更便于问题的排查。

### 功能

1. [更加便捷的集成OpenTelemetry，收集可观性数据](#集成OpenTelemetry)；
2. Traces、Logs和Metrics的查询统计;
3. 可观测性的开放能力。

## 集成

安装包

```csharp
dotnet add package Masa.Contrib.StackSdks.Tsc
```

### 入门

```csharp
var options = new MasaObservableOptions
{
    //服务名称，必须配置
    ServiceName = "my-project-api",
    //环境，例如开发、测试、生产等，可选配置，默认为空
    ServiceNameSpace = "development",
    //服务版本号，可选配置，默认为空
    ServiceVersion = "1.0"
};
//otlpUrl默认为 "http://localhost:4717"
builder.Services.AddObservable(builder.Logging, options);
```
> [OTLP服务相关部署和配置](https://opentelemetry.io/docs/collector/)

应用程序启动后，该应用在进行可观测性监测时，服务名称为：`my-project-api`，环境为：`development`,服务版本号为：`1.0`,这些参数在服务启动时就已经被确定，运行期间不能被更改，如果想了解更多的参数配置，可参考[Resource Semantic Conventions](https://github.com/open-telemetry/opentelemetry-specification/tree/main/specification/resource/semantic_conventions#service)

### 高阶用法

1. 如果想完全自定义可观测性的配置，可以参考[OpenTelemetry for ASP.NET Core](https://opentelemetry.io/docs/instrumentation/net/getting-started/);
2. 在我们的框架范围内实现自定义配置：

#### Logs

以下示例为单独添加Log遥测数据收集的例子，还添加了应用的自定义标识`custom-key-1`和`custom-key-2`

```csharp
builder.Logging.AddMasaOpenTelemetry(options => {
    var resourceBuilder = ResourceBuilder.CreateDefault();
    resourceBuilder.AddService("my-project-api", 
            serviceNamespace:"development", 
            serviceVersion:"1.0");
    resourceBuilder.AddAttributes(new KeyValuePair<string, object>[]{
        new KeyValuePair<string, object>("custom-key-1","custom-value-1"),
        new KeyValuePair<string, object>("custom-key-2","custom-value-2")
    });

    options.SetResourceBuilder(resourceBuilder);
    options.AddOtlpExporter(ops =>
    {
        ops.Endpoint = new Uri("https://otlpservice.io");
    });            
})
```

实例默认将遥测数据导出到OTLP服务，如果您想更改导出器，只需替换`options.AddOtlpExporter`为您所需要的即可


#### Traces

```csharp
builder.Services.AddMasaTracing(builder =>
{
    var resourceBuilder = ResourceBuilder.CreateDefault();
    ....

    /* blazor应用和API应用在链路的过滤条件上稍微有些区别，
     * 两者都会过滤js、css、image、icon和字体资源文件以及/swagger(API文档)和/healthz(服务健康检查)两个资源，
     * blazor额外过滤以/_blazor、/_content和/negotiate开始的请求
     */
    if (isBlazor)
        builder.AspNetCoreInstrumentationOptions.AppendBlazorFilter(builder);
    else
        builder.AspNetCoreInstrumentationOptions.AppendDefaultFilter(builder);

    builder.BuildTraceCallback = options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddOtlpExporter();
    };
});
```

`resourceBuilder`创建参考Logs，由于在.NET 6中，[基于`SignalR`的长链接，在链路处理上存在无法区分链路的问题](https://github.com/dotnet/aspnetcore/issues/29846)，当前我们采用了忽略长链接的链路，后续官方版本升级时，我们也会跟进和修正该问题。

#### Metrics

```csharp
builder.Services.AddMasaMetrics(builder =>
{
    var resourceBuilder = ResourceBuilder.CreateDefault();
    ....

    builder.SetResourceBuilder(resourceBuilder);    
    builder.AddRuntimeMetrics();
    builder.AddOtlpExporter();    
});
```

该示例集成了`OpenTelemetry`并添加了`RuntimeMetrics`运行时`Metrics`监测，如果需要添加其它`Metrics`的监测，自己引入包添加监测即可。


## 遥测数据查询

当前版本的存储实现为：
`Logs`和`Traces`存储于`Elasticseach`,`Metrics`存储于`Prometheus`，所以目前版本的`Logs`和`Traces`的查询API也是基于`Elasticsearch`实现。另外使用该功能时，需要引入包：

```csharp
dotnet add package Masa.Contrib.StackSdks.Tsc.Elasticsearch
```

### Logs查询

1. `Task<PaginatedListBase<LogResponseDto>> ListAsync(BaseRequestDto query)`：日志列表分页查询，采用`Elasticsearch`的默认分页方式读取，返回数据最多不超过10000条，原因详见[Paginate search result](https://www.elastic.co/guide/en/elasticsearch/reference/current/paginate-search-results.html)；
2. `Task<IEnumerable<MappingResponseDto>> GetMappingAsync()`：日志结构映射mapping查询，用来作为自定义查询的条件；
3. `Task<object> AggregateAsync(SimpleAggregateRequestDto query)`：日志聚合统计功能，当前版本限于`Elasticsearch`，目前实现了几种简单的统计。


示例：

1. 获取日志mapping

```csharp
var mappings = await _logService.GetMappingAsync();
```

2. 日志列表分页查询

```csharp
var query = new BaseRequestDto
{
    Page = 1,
    PageSize = 10,
    Conditions = new FieldConditionDto[] {
            new FieldConditionDto{ Name="Attributes.Name",
            Type= ConditionTypes.Equal, Value="UserAuthorizationFailed" }
        },
    Service ="my-project-api",
};

var result = await _logService.ListAsync(query);
```
该示例展示了查询日志列表，请求类`BaseRequestDto`包含了`TraceId`（链路id全匹配）、`Service`（服务名称全匹配）、`Instance`（服务实例全匹配）、`Endpoint`（服务类型为http服务时，请求的url路径全匹配）、`Start`（开始时间）、`End`（结束时间）和`Keyword`（全文模糊匹配）几个通用查询，如果需要更多的查询条件，可以使用 `Conditions` 和 `RawQuery` （json格式的Elasticsearch的原始查询条件）添加更多过滤条件。

3. 聚合统计

```csharp
var query = new SimpleAggregateRequestDto
{
    Service = "my-project-api",
    Name = ElasticConstant.ServiceName,
    Type = AggregateTypes.Count
};
var result = await _logService.AggregateAsync(query);
```
示例返回服务名称为"my-project-api"的日志总条数，目前每次查询只支持一级聚合，嵌套聚合目前不支持，聚合类型支持`Count`、`Sum`、`Avg`、`DistinctCount`、`DateHistogram`和`GroupBy`，`Sum`和`Avg`必须为数值类型的字段，`DateHistogram`必须是对日期类型的字段，其它如果是字符串类型，必须为`keyword`类型。

> 返回结果类型：`GroupBy` 返回 `IEnumerable<string>`,`DateHistogram` 返回 `IEnumerable<KeyValuePair<double,string>>`，其它返回`double`。

### Traces查询

1. `Task<IEnumerable<TraceResponseDto>> GetAsync(string traceId)`：根据`traceId`获取整个链路的所有trace信息
2. `Task<PaginatedListBase<TraceResponseDto>> ListAsync(BaseRequestDto query)`：获取trace列表
3. `Task<object> AggregateAsync(SimpleAggregateRequestDto query) trace聚合查询`：参见log聚合统计
4. `void GetAll(BaseRequestDto query, Action<IEnumerable<TraceResponseDto>> resultAction)`：获取指定条件的所有trace数据

### Metrics查询

目前采用的标准[`HTTP API`](../utils/data/prometheus.md)。