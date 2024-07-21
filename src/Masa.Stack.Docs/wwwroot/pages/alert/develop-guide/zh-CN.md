# 开发指南

## 可观测性（OpenTelemetry）接入

1. 安装包

   ```shell 终端
   dotnet add package Masa.Contrib.StackSdks.Tsc
   ```

2. 修改`appsettings.json`，配置所需参数

   ```json appsettings.json
   {
     "Masa": {
       "Observable": {
         "ServiceName": "masa-alert-service",
         "ServiceNameSpace": "Development",
         "ServiceVersion": "1.0.0",
         "OtlpUrl": ""//填写实际的OpenTelemetry地址,
         ""
       }
     }
   }
   ```

3. 接入可观测性，会自动采集数据到OpenTelemetry

   ```csharp Program.cs
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddObservable(builder.Logging, builder.Configuration);
   ```

## 告警处理第三方接入

1. 添加网络钩子

   请参考[网络钩子](stack/alert/use-guide/web-hook#创建/编辑)

2. 告警处理转派第三方

    请参考[处理告警](stack/alert/use-guide/alarm-history#处理告警)

3. 网络钩子接入

   ```csharp l:7
   public class WebHookTestDto
   {
       //告警Id
       public Guid AlarmHistoryId { get; set; }
       //处理人
       public Guid Handler { get; set; }
       //密钥  TODO 安全起见，第三方应当接收并校验这个值与预留值是否一致
       public string SecretKey { get; set; }
   }
   
   [RoutePattern("{id}/test", StartWithBaseUri = true, HttpMethod = "Post")]
   public async Task TestAsync(IEventBus eventBus, Guid id, [FromBody] WebHookTestDto inputDto)
   {
       
   }
   ```
