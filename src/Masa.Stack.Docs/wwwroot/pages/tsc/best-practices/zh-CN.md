# 最佳实践

## 接入 MASA Stack 的可观测性

前提必须已经部署了Masa Stack和相关的服务(OTLP,Elasticsearch和Prometheus)

1. 安装包：

```csharp
dotnet add package Masa.Contrib.StackSdks.Tsc.OpenTelemetry
```

2. 接入
```csharp
var builder = WebApplication.CreateBuilder(args);

...

builder.Services.AddObservable(builder.Logging, new MasaObservableOptions
{
    //环境名称，可空，多个环境时，建议填写
    ServiceNameSpace = "Develop",
    //服务版本，可空
    ServiceVersion = "1.0",
    //服务名称，如果已接入了Masa Stack，必须与Masa PM相应的应用唯一标识相同
    ServiceName = "tsc-service",
    //服务分类,可控，默认为General
    Layer = "masa-stack",
    //服务实例,可控，默认为随机的Guid
    ServiceInstanceId = "instance-1"
}, "http://127.0.0.1:4717", true);

 ```

 `otlpUrl`,可空，默认为`http://localhost:4717`,OTLPExporter，默认协议为`Grpc`，如果需要更换为`Http`，请参考更加详细的配置；

 `isInterruptSignalRTracing`,默认`false`,是否强制中断长连接的`Trace`,如果为`false`,使用了长连接后，会存在在一个长连接内，所有的请求链路都是同一个链路，导致出现问题后，不便于排查。

 ## 验证是否接入成功

 1. 接入 `MASA.Alert`，在团队首页，选择项目所在的团队后，首页会出现对应的项目信息，则代表接入成功。

    ![接入成功团队验证图](https://cdn.masastack.com/stack/doc/tsc/best-practices/team-succeed.png) 

2. 接入 `masa-alert-service-admin`，追踪页面，服务下拉选项，搜索对应的应用名称，能够找到对应的数据，则代表接入成功。
    
    ![接入成功追踪验证图](https://cdn.masastack.com/stack/doc/tsc/best-practices/trace-succeed.png)
