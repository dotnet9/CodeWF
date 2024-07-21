# 开始使用

`Job` 为用户设置/定义好的一个作业的配置。

## 列表

调度 `Job` 列表以卡片形式展现，支持糊搜索、分页、筛选等功能。

* `Job` 状态：为最后一次任务运行的状态，不包含等待运行状态。

  ![jobs](https://cdn.masastack.com/stack/doc/scheduler/jobs.png)


## 新增

### 选择 Job 类型

支持 `Job`、`HTTP`、`Dapr Service Invocation`

![job-add-type](https://cdn.masastack.com/stack/doc/scheduler/job-add-type.png)

**使用方式**

   - [Job 应用](stack/scheduler/use-guide/scheduler-job-app)

   - [HTTP](stack/scheduler/use-guide/scheduler-http)

   - [Dapr Service Invocation](stack/scheduler/use-guide/scheduler-dapr)


### Job 基本信息和策略

**三种 `Job` 类型的策略**

![job-edit-baseInfo](https://cdn.masastack.com/stack/doc/scheduler/job-edit-baseInfo.png)

**告警**

点击`是否告警`弹出[告警规则](stack/alert/use-guide/alarm-rule)弹窗，规则自动生成，可以按需调整

![job-edit-alert](https://cdn.masastack.com/stack/doc/scheduler/job-edit-alert.png)

| 字段 | 描述 |
| --- | --- |
| **调度类型** | **手动运行**：只能通过后台点击启动或者通过 `SDK` 调用 `StartAsync` 接口启动。<br/>**Cron**：会将 `Job` 注册到 `Quartz`，由 `Quartz` 根据 `Cron` 表达式触发 `Start` 事件。 |
| **路由策略** | **轮询**：`Server` 会将 `Job` 进行轮询的方式调度给 `Worker` 执行。<br/>**指定**：`Server` 会将 `Job` 通过指定的方式发送给指定的 `Worker`。 |
| **调度过期策略** | **立即运行一次**：`Server` 会在程序启动时进行立即执行一次的补偿。<br/>**自动补偿**：`Server` 会在程序启动时进行计算，得到需要补偿的时间，通过参数把时间传递给 `DLL/接口`，`DLL/接口` 需要拿到参数里的时间执行各自的逻辑。<br/>**忽略**：`Server` 不会进行补偿。 |
| **阻塞处理策略** | **等待上次任务完成**：`Server` 会等待这个 `Job` 上次任务执行完成，再执行下一次。<br/>**同时运行**：这个 `Job` 当前任务触发将会直接运行。<br/>**丢弃当前任务**：`Server` 将会把当前任务丢弃，直到上一次任务执行结束。<br/>**中断上次运行并即执行**：`Server` 将会把上一次任务执行强行中断，并且立即执行此次任务。注意：使用此策略，执行的 `DLL` 请用事务。如是 `HTTPJob`，需要自己处理异常。 |
| **超时策略** | **超时后执行失败策略**：会将超时的任务视作失败，然后按照失败策略进行重试。<br/>**超时后忽略**：会将超时的任务忽略，让其继续等待，直到有结果为止。 |
| **失败策略** | **自动**：会根据配置的重试次数和重试间隔，进行重试。达到次数后，还是失败，则会判定为任务失败。<br/>**手动**：会直接判定为任务失败。 |

1. 操作 `Job`

2. 点击 `⋮`

   * 运行
   
   * 编辑
   
   * 禁用

     ![scheduler_job](https://cdn.masastack.com/stack/doc/scheduler/rc1/scheduler_job.png)

3. 点击 `Job`

   * 任务日志

	 记录任务执行过程中发生的事件和活动的信息。它通常包含任务开始和结束的时间，执行任务的人员，任务的进展情况，任何问题。
   
   * 链路追踪
	
	 记录和分析分布式系统中请求路径和性能，可以帮助开发人员和运维人员识别和解决系统中的性能问题和错误。链路追踪通过在请求的不同组件之间添加唯一标识符（例如 `Trace ID`），并在请求的整个生命周期中跟踪这些标识符来实现。[更多](stack/tsc/introduce)

   * 重跑
	   
     重新运行此任务。

     ![scheduler_job](https://cdn.masastack.com/stack/doc/scheduler/rc1/scheduler_task.png)
