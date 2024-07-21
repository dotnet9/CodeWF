# SDK 示例

## 简介

通过注入 `IAlertClient` 接口，调用对应 Service 获取 Alert SDK 提供的能力。

## 服务介绍

Alert SDK 包含以下服务：

```csharp
IAlertClient
   ├── AlarmRuleService                    告警规则服务
```

## 使用介绍

### 安装依赖包

``` shell 终端
dotnet add package Masa.Contrib.StackSdks.Alert
```

### 注册相关服务

```csharp program.cs
builder.Services.AddAlertClient("https://alertservice.com");
```

> `https://alertservice.com` 需要替换为真实的 Alert 后台服务地址。

### 依赖注入 IAlertClient

```csharp program.cs
var app = builder.Build();
   
app.MapGet("/GetAlarmRule", ([FromServices] IAlertClient alertClient, Guid id) =>
{
   return alertClient.AlarmRuleService.GetAsync(id);
});
   
app.Run();
```

## 场景

任务调度中心（MASA.Scheduler）的 Job 需要告警时通过调用告警中心（MASA.Alert）的 SDK 自动创建告警规则并进行管理。

### 创建告警规则

示例只介绍部分参数用法。

```csharp
var whereExpression = $@"{{""bool"":{{""must"":[{{""term"":{{""Attributes.JobId.keyword"":""{jobId}""}}}},{{""term"":{{""SeverityText.keyword"":""Error""}}}}]}}}}";
var ruleExpression = @"{""Rules"":[{""RuleName"":""CheckWorkerErrorJob"",""ErrorMessage"":""Log with error level."",""ErrorType"":""Error"",""RuleExpressionType"":""LambdaExpression"",""Expression"":""JobId > 0""}]}";
var alarmRule = new AlarmRuleUpsertModel
{
   //规则类型，目前支持日志和指标
   Type = AlarmRuleType.Log,
   //规则显示名称
   DisplayName = displayName,
   //项目ID
   ProjectIdentity = MasaStackConstant.SCHEDULER,
   //应用ID
   AppIdentity = MasaStackConfig.GetServerId(MasaStackConstant.SCHEDULER, "worker"),
   //检查频率
   CheckFrequency = new CheckFrequencyModel
   {
      Type = AlarmCheckFrequencyType.Cron,
      CronExpression = "0 0/10 * * * ? ",
      FixedInterval = new TimeIntervalModel
      {
         IntervalTimeType = TimeType.Minute
      }
   },
   //是否启用
   IsEnabled = true,
   //监控项配置
   LogMonitorItems = new List<LogMonitorItemModel> {
      new LogMonitorItemModel {
         Field = "Attributes.JobId",
         AggregationType = LogAggregationType.Count,
         Alias = "JobId"
      }
   },
   //日志筛选条件
   WhereExpression = whereExpression,
   //触发规则，包含规则表达式和通知策略
   Items = new List<AlarmRuleItemModel> {
      new AlarmRuleItemModel {
         Expression = ruleExpression,
         AlertSeverity = AlertSeverity.High
      }
   }
};

var alarmRuleId = await AlertClient.AlarmRuleService.CreateAsync(alarmRule);
```

### 更新告警规则

```csharp
await AlertClient.AlarmRuleService.UpdateAsync(alarmRule);
```

### 启用/禁用告警规则

```csharp
await AlertClient.AlarmRuleService.SetIsEnabledAsync(alarmRuleId, isEnabled);
```