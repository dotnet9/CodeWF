# HTTP

## 安装包

```shell 终端
dotnet add package Masa.Contrib.StackSdks.Scheduler
```

## 手动创建

![填写调度信息](https://cdn.masastack.com/stack/doc/scheduler/scheduler_job_base.png)

![填写调度信息2](https://cdn.masastack.com/stack/doc/scheduler/scheduler_job_http.png)

| 类型     | 描述                                                                                                                                  |
|----------|---------------------------------------------------------------------------------------------------------------------------------------|
| 请求类型 | `HTTP` 请求类型（`GET`，`POST`，`PUT`，`DELETE`，`HEAD`）                                                                             |
| 异步模式 | 开启后，http请求成功不会处理结果，需要业务方手动调用sdk通知scheduler任务执行结果                                                      |
| 请求地址 | 调度请求的接口地址                                                                                                                    |
| 请求参数 | 接口参数                                                                                                                              |
| 校验条件 | **默认响应码 200**：接口 `HTTP` 响应码返回是否为 `200` <br/> **内容包含**：与**校验内容**配合使用<br/> **内容不包含**：与**校验内容**配合使用 |

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
   public class SchedulerHttpController : ControllerBase
   {
       private readonly ISchedulerClient _schedulerClient;
   
       public SchedulerHttpController(ISchedulerClient schedulerClient)
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
               JobType = JobTypes.Http,
               CronExpression = "",
               OperatorId = Guid.Empty,
               HttpConfig = new SchedulerJobHttpConfig
               {
                   HttpMethod = HttpMethods.GET,
                   RequestUrl = "",
                   HttpParameters = new List<KeyValuePair<string, string>>(),
                   HttpHeaders = new List<KeyValuePair<string, string>>(),
                   HttpBody = "",
                   HttpVerifyType = HttpVerifyTypes.StatusCode200,
                   VerifyContent = ""
               }
           };
           var jobID = await _schedulerClient.SchedulerJobService.AddAsync(request);
           return new JobRegisterResult(jobID);
       }
   } 
   
   public record JobRegisterResult(Guid JobID);
   ```

   | 属性                | 描述                                      |
   |---------------------|-------------------------------------------|
   | **ProjectIdentity** | [项目](stack/pm/introduce) `ID`           |
   | **Name**            | `Job` 的名称                              |
   | **JobType**         | `Job` 的类型（`JobTypes.HTTP` 为 `HTTP`） |
   | **CronExpression**  | `Cron` 表达式（`Job` 执行的周期）         |
   | **OperatorId**      | 操作人/创建人                             |
   | **HttpMethod**      | 请求类型                                  |
   | **RequestUrl**      | 请求地址                                  |
   | **HttpParameters**  | 接口参数（`Query`）                       |
   | **HttpBody**        | 接口参数 (`Content`)                      |
   | **HttpVerifyType**  | 校验条件                                  |
   | **VerifyContent**   | 校验内容                                  |

## 异步模式

> 异步模式适合请求耗时的HTTP Job，开启后Scheduler请求业务方接口成功不会处理结果。业务方接口可以将要执行的逻辑放到BackgroundJob里，BackgroundJob执行完再调用SDK通知Scheduler结果

1. 业务方接口示例

    ```csharp
    public class DemoService : ServiceBase
    {
        public async Task CheckAsync([FromQuery] Guid taskId)
        {
            var args = new CheckJobArgs()
            {
                TaskId = taskId
            };
            //将耗时操作交由后台任务来执行
            await BackgroundJobManager.EnqueueAsync(args);
        }
    }
   ```

2. BackgroundJob类示例

    ```csharp
    public class CheckJob : BackgroundJobBase<CheckJobArgs>
    {
        private readonly ISchedulerClient _schedulerClient;
    
        public CheckJob(ILogger<BackgroundJobBase<CheckJobArgs>>? logger, ISchedulerClient schedulerClient) : base(logger)
        {
            _schedulerClient = schedulerClient;
        }
    
        protected override Task PreExecuteAsync(CheckJobArgs args)
        {
            Logger?.LogDebug("----- background task running and jobArgs: {JobArgs}", args.ToJson());
            return Task.CompletedTask;
        }
    
        protected override Task ExecutingAsync(CheckJobArgs args)
        {
            // 你的业务代码
    
            return Task.CompletedTask;
        }
    
        protected override async Task PostExecuteAsync(CheckJobArgs args)
        {
            Logger?.LogDebug("-----The end of the background task, jobArgs: {JobArgs}", args.ToJson());
    
            //调用Scheduler SDK 通知结果
            var request = new NotifySchedulerTaskRunResultRequest
            {
                TaskId = args.TaskId,
                Status = TaskRunResultStatus.Success
            };
    
            await _schedulerClient.SchedulerTaskService.NotifyRunResultAsync(request);
        }
    }

    public class CheckJobArgs
    {
        public Guid TaskId { get; set;}
    }
   ```
