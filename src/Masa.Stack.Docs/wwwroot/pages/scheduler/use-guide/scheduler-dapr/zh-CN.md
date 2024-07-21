# Dapr Service Invocation

## 安装包

   ```shell 终端
   dotnet add package Masa.Contrib.StackSdks.Scheduler
   ```

## 手动创建

![填写调度信息](https://cdn.masastack.com/stack/doc/scheduler/rc1/scheduler_dapr_insert.png)

![填写调度信息2](https://cdn.masastack.com/stack/doc/scheduler/rc1/scheduler_dapr_insert_2.png)

| 类型         | 描述                                                      |
|--------------|-----------------------------------------------------------|
| Service 应用 | 调用项目的服务                                            |
| Namespace    | 目前暂无用到此参数                                        |
| Method Name  | 接口地址（无需 `Host` 部分例如 /api/test）                |
| 请求类型     | `HTTP` 请求类型（`GET`，`POST`，`PUT`，`DELETE`，`HEAD`） |
| Data         | 参数内容                                                  |

## API创建

1. 注册 `Scheduler` 服务

   ```csharp Program.cs
   builder.Services.AddSchedulerClient("schedulers 服务地址");
   ```

2. 注册一个 `Job` 应用示例

   ```csharp
   using Masa.BuildingBlocks.StackSdks.Scheduler;
   using Masa.BuildingBlocks.StackSdks.Scheduler.Enum;
   using Masa.BuildingBlocks.StackSdks.Scheduler.Model;
   using Masa.BuildingBlocks.StackSdks.Scheduler.Request;
   using Microsoft.AspNetCore.Mvc;
   
   /// <summary>
   /// 一个测试的任务调度的Controller
   /// </summary>
   [ApiController]
   [Route("[controller]/[action]")]
   public class SchedulerDaprController : ControllerBase
   {
       private readonly ISchedulerClient _schedulerClient;
   
       public SchedulerDaprController(ISchedulerClient schedulerClient)
       {
           _schedulerClient = schedulerClient;
       }
   
       [HttpPost]
       public async Task<JobRegisterResult> Register()
       {
           var request = new AddSchedulerJobRequest
           {
               ProjectIdentity = "",
               Name = "",
               JobType = JobTypes.DaprServiceInvocation,
               CronExpression = "",
               OperatorId = Guid.Empty,
               DaprServiceInvocationConfig = new SchedulerJobDaprServiceInvocationConfig
               {
                   DaprServiceIdentity = "",
                   MethodName = "/api/test",
                   HttpMethod = HttpMethods.GET,
                   Data = ""
               }
           };
           var jobID = _schedulerClient.SchedulerJobService.AddAsync(request);
           return new JobRegisterResult(jobID);
       }
   }
   
   public record JobRegisterResult(Guid JobID);
   
   ```
  
   | 属性                    | 描述                                                                          |
   |-------------------------|-------------------------------------------------------------------------------|
   | **ProjectIdentity**     | [项目](stack/pm/introduce) `ID`                                               |
   | **Name**                | `Job` 的名称                                                                  |
   | **JobType**             | `Job` 的类型（`JobTypes.DaprServiceInvocation` 为 `Dapr Service Invocation`） |
   | **CronExpression**      | `Cron` 表达式（`Job` 执行的周期）                                             |
   | **OperatorId**          | 操作人/创建人                                                                 |
   | **DaprServiceIdentity** | `Service` 应用（`ID`）                                                        |
   | **MethodName**          | 请求地址                                                                      |
   | **HttpMethod**          | 请求类型                                                                      |
   | **Data**                | 接口参数 (`Content`)                                                          |

