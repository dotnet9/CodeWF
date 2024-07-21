# 基本概念

## 监测原理

TSC接入[OpenTelemetry](https://opentelemetry.io/)实现了可观测性，采集了Trace、Log和Metric数据，用来快速诊断系统和应用的故障、问题，从而更快的发现和解决问题。

系统整体的运行结构如下：

![tsc运行结构图](https://cdn.masastack.com/stack/doc/tsc/use-guide/basic-concepts/structure.png)

### 概述

当前阶段，我们的应用统一通过`OTLPExporter`将可观测性数据传输到`Otel`服务，`Otel`服务将`Logs`和`Traces`存储到`Elasticsearch`,`Metrics`数据存储到`Prometheus`,
TSC服务再通过`Elasticsearch`来查询`Logs`和`Traces`数据，通过`Prometheus HTTP API`查询`Metrics`数据。

采用`OTLPExporter`为了便于后期扩展和适配更多的存储方案，例如存储第三方云平台，或更加适合对应数据的处理的存储。

### 数据

`Logs`是应用程序记录的详细日志信息，可以包含某个方法的手动调试打印信息，也可以是程序异常的详细信息

`Traces`为应用程序的链路请求信息，完整的包含了用户一次请求的详细请求过程信息，当前阶段支持`Http`和`Database`的链路记录，每个链路会有唯一的`TraceId`标识，每个链路里面的`Http`请求或`Database`都有唯一标识`SpanId`，在相关的链路处理过程中产生的日志也会包含这些信息

`Metrics`应用性能检测指标数据，例如常见的，接口请求数量，接口请求耗时，接口成功率，服务实例负载，服务满意度等，这里的指标数据内容跟实际的需要可能会存在一定的差别，指标详细请参考[.NET metrics](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics)

## 指标取数原理

### http指标

|  名称  |  类型  |  说明  |
|  ------  |  --------  |  ---------  |
|  http.client.duration |  histogram  |  http client 耗时[直方图](https://cloud.tencent.com/developer/article/1495303)  |
|  http.server.duration  |  histogram  |  http server 耗时[直方图](https://cloud.tencent.com/developer/article/1495303)  |

这两个指标均为`Openteletetry`提供，分别为监测`HTTP`客户端和服务端的请求耗时，由于我们存储在`Prometheus`，指标会做转化为`http_client_duration`和`http_server_duration`，由于是直方图，响应的指标实际存储为三个指标数据：

|  名称  |  类型  |  说明  |
|  ------  |  --------  |  ---------  |
|  http_server_duration_bucket |  guage  |  实际的请求耗时 |
|  http_server_duration_sum  |  counter  |  当前时间总的请求耗时  |
|  http_server_duration_count  |  counter  |  当前时间总的请求数量  |

基于`HTTP`的指标计算，基于上述三个实际指标进行计算

### .NET EventCounters

ASP.NET的基础指标，参见[Well-known EventCounters in .NET](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters?source=recommendations)。本阶段未将该类指标重点处理，如果大家有兴趣或者有需求，可参考官方文档进行详细了解。

### Dapr

本版本Dapr指标只做了接入，还未做正式的应用，采集的指标详细可参看[Dapr metrics](https://github.com/dapr/dapr/blob/master/docs/development/dapr-metrics.md)，以及[查看Dapr的指标](https://docs.dapr.io/operations/monitoring/metrics)
