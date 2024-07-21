# SDK 示例

## 简介

`MASA.PM` 提供了 SDK 以支持获取 `PM` 系统的数据。通过引入 `Masa.Contrib.StackSdks.Pm` SDK，可以调用 `PM` 的 `EnvironmentService`、`ClusterService`、`ProjectService`、`AppService` 来获取环境数据、集群数据、项目数据和应用数据。

``` plain
IPmClient
├── EnvironmentService                  环境服务
├── ClusterService                      集群服务
├── ProjectService                      项目服务
├── AppService                          应用服务
```

## 场景

`MASA.Auth` 切换环境时需要调用 `MASA.PM` 的 SDK 去获取相应的环境列表数据。

`MASA.Auth` 配置应用的权限时需要通过 `MASA.PM` 的 SDK 去获取相应的应用数据。

### 示例

``` ps
dotnet add package Masa.Contrib.StackSdks.Pm
```

``` csharp
builder.Services.AddPmClient("Pm服务地址");

var app = builder.Build();

app.MapGet("/GetProjectApps", ([FromServices] IPmClient pmClient) =>
{
    return pmClient.ProjectService.GetProjectAppsAsync("development");
});

app.Run();
```
