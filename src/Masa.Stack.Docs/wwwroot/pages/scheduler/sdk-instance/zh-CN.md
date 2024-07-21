## 简介

通过注入 `ISchedulerClient` 接口，调用对应 `Service` 获取 `Scheduler SDK` 提供的能力。

## 服务介绍

`Scheduler SDK` 包含一下几个大类的服务

```csharp
ISchedulerClient
├── SchedulerJobService             调度Job服务
├── SchedulerTaskService            调度任务服务
```

## 使用介绍

1. 安装依赖包

   ```shell 终端
   dotnet add package Masa.Contrib.StackSdks.Scheduler
   ```

2. 注册 Scheduler 服务

   ```csharp Program.cs
   builder.Services.AddSchedulerClient("http://schedulerservice.com");
   ```

   > `http://schedulerservice.com` 需要替换为真实的 `Scheduler` 后台服务地址

3. 用例

   * [注册 Job 应用](stack/scheduler/use-guide/scheduler-job-app/#API创建)

   * [注册 HTTP](stack/scheduler/use-guide/scheduler-http/#API创建)

   * [注册 Dapr](stack/scheduler/use-guide/scheduler-dapr/#API创建)

