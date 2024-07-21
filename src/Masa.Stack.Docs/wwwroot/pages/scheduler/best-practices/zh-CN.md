---
title: 最佳实践
date: 2023/03/13 13:22:00
---

### MASA.Scheduler 实现分布式任务调度

1. 使用 `HTTP` 类型的 `Job`

    在 `Scheduler` 管理端或者使用 `SDK` 创建 `HTTP` 类型的 `Job`

      * 管理端创建 `Job`，请参考[使用指南-调度 Job](stack/scheduler/use-guide/scheduler-job)
      
      * `SDK` 创建 `Job`，请参考 [SDK 示例](stack/scheduler/sdk-instance)

2. 使用 `Job` 应用类型的 `Job`

   * 创建一个 `NETCore` 类库

   * 安装依赖包
     ```shell 终端
     dotnet add package Masa.Contrib.StackSdks.Scheduler
     ```

   * 创建自己的业务 `Job` 类，需要继承 `SchedulerJob` 类
     ```csharp
     public class MyExecuteJob : SchedulerJob
     {
         public override async Task<object?> ExcuteAsync(JobContext context)
         {
             var myParameter = context.ExcuteParameters[0];//注册job时配置的传递参数
             var jobId = context.JobId;//调度中心的JobId
             var taskId = context.TaskId;//调度中心的TaskId
             var executionTime = context.ExecutionTime;//补偿时间
             // 你的业务

             await Task.CompletedTask;

             return "Success";
         }
     }
     ```
 
   * 将你的 `Job` 类库发布打包，上传到 `Scheduler` 源文件管理，源文件上传请参考[使用指南-调度 Job](stack/scheduler/use-guide/scheduler-job)

   * 通过 `Scheduler` 管理端或者 `SDK` 创建一个 `Job` 应用类型的 `Job`，并配置好指定的程序集和执行类

      * 管理端创建 `Job`，请参考[使用指南-调度 Job](stack/scheduler/use-guide/get-started)
      
      * `SDK` 创建 `Job`，请参考 [SDK 示例](stack/scheduler/sdk-instance)
