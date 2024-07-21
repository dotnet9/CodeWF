# SDK 示例

## 简介

通过注入 `IMcClient` 接口，调用对应 `Service` 获取 `MC SDK` 提供的能力。

## 服务介绍

`Mc SDK` 包含一下几个大类的服务

```csharp
IMcClient
├── ChannelService                  渠道服务
├── MessageTaskService              消息任务服务
├── MessageTemplateService          消息模板服务
├── ReceiverGroupService            收件人组服务
├── WebsiteMessageService           站内信服务
```

## 使用介绍

### 安装依赖包

``` shell 终端
dotnet add package Masa.Contrib.StackSdks.Mc
```

### 注册 MC 服务

```csharp Program.cs
builder.Services.AddMcClient("http://mcservice.com");
```

> `http://mcservice.com` 需要替换为真实的 `MC` 后台服务地址

### 消息发送

:::: code-group
::: code-group-item 给外部用户发送普通消息(邮箱)
```csharp
var app = builder.Build();

app.MapGet("/SendEmail", async ([FromServices] IMcClient mcClient) =>
{
    var message = new SendOrdinaryMessageByExternalModel
    {
        ChannelCode = "渠道代码",
        ChannelType = ChannelTypes.Email,
        ReceiverType = SendTargets.Assign,
        MessageInfo = new MessageInfoUpsertModel
        {
            Title = "邮件标题",
            Content = "邮件内容"
        },
        Receivers = new List<ExternalReceiverModel>
        {
            new ExternalReceiverModel
            {
                ChannelUserIdentity = "邮箱地址"
            }
        }
    };

    await mcClient.MessageTaskService.SendOrdinaryMessageByExternalAsync(message);
});

app.Run();
```
:::

::: code-group-item 给 `Auth` 用户发送模板消息(短信)
```csharp
var app = builder.Build();

app.MapGet("/SendSms", async ([FromServices] IMcClient mcClient) =>
{
    var message = new SendTemplateMessageByInternalModel
    {
        ChannelCode = "渠道代码",
        ChannelType = ChannelTypes.Sms,
        ReceiverType = SendTargets.Assign,
        TemplateCode = "模板代码",
        Receivers = new List<InternalReceiverModel>
        {
            new InternalReceiverModel
            {
                SubjectId = Guid.Parse("你的用户id"),
                Type = MessageTaskReceiverTypes.User
            }
        }
    };

    await mcClient.MessageTaskService.SendTemplateMessageByInternalAsync(message);
});

app.Run();
```
:::

::: code-group-item 发送站内信广播
```csharp
var app = builder.Build();

app.MapGet("/SendWebsiteMessage", async ([FromServices] IMcClient mcClient) =>
{
    var message = new SendTemplateMessageByInternalModel
    {
        ChannelCode = "渠道代码",
        ChannelType = ChannelTypes.WebsiteMessage,
        ReceiverType = SendTargets.Broadcast,
        TemplateCode = "模板代码",
    };
    
    await mcClient.MessageTaskService.SendTemplateMessageByInternalAsync(message);
});

app.Run();
```
:::

::: code-group-item 给用户发送 `APP` 消息
```csharp
var app = builder.Build();

app.MapGet("/SendAppMessage", async ([FromServices] IMcClient mcClient) =>
{
    var message = new SendOrdinaryMessageByInternalModel
    {
        ChannelCode = "渠道代码",
        ChannelType = ChannelTypes.App,
        ReceiverType = SendTargets.Assign,
        Receivers = new List<InternalReceiverModel>
        {
            new InternalReceiverModel
            {
                SubjectId = Guid.Parse("你的用户id"),
                Type = MessageTaskReceiverTypes.User
            }
        },
        MessageInfo = new MessageInfoUpsertModel
        {
            Title = "标题",
            Content = "内容",
            ExtraProperties = new ExtraPropertyDictionary()//透传内容
        }
    };

    await mcClient.MessageTaskService.SendOrdinaryMessageByInternalAsync(message);
});

app.Run();
```
::: 
::::

#### 给内部用户发送普通消息

SendOrdinaryMessageByInternalAsync

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **ChannelCode**                          | 渠道编码/渠道 `ID`                                                                                  
| **ChannelType**                          | 渠道类型（`SMS`：短信，`Email`：邮箱，`WebsiteMessage`：站内信，`APP`：应用程序）                  
| **ReceiverType**                         | 发送类型（`Assign`：指定，`Broadcast`：广播）  
| **MessageInfo**                          | 普通消息内容，详见MessageInfoModel                                                          
| **Variables**                            | 模板参数
| **Receivers[]**                          | 内部收件人，详见InternalReceiverModel                                   
| **SendRules**                            | 发送规则，详见SendRuleModel       
| **ExtraProperties**                      | 扩展属性，App消息时指定IsWebsiteMessage=true可以同时生成站内信

#### 给内部用户发送模板消息

SendTemplateMessageByInternalAsync

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **ChannelCode**                          | 渠道编码/渠道 `ID`                                                                                  
| **ChannelType**                          | 渠道类型（`SMS`：短信，`Email`：邮箱，`WebsiteMessage`：站内信，`APP`：应用程序）                  
| **ReceiverType**                         | 发送类型（`Assign`：指定，`Broadcast`：广播）  
| **TemplateCode**                         | 模板编码/模板 `ID`
| **Variables**                            | 模板参数
| **Receivers[]**                          | 内部收件人，详见InternalReceiverModel      
| **SendRules**                            | 发送规则，详见SendRuleModel    
| **ExtraProperties**                      | 扩展属性

#### 给外部用户发送普通消息

SendOrdinaryMessageByExternalAsync

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **ChannelCode**                          | 渠道编码/渠道 `ID`                                                                                  
| **ChannelType**                          | 渠道类型（`SMS`：短信，`Email`：邮箱，`WebsiteMessage`：站内信，`APP`：应用程序）                  
| **ReceiverType**                         | 发送类型（`Assign`：指定，`Broadcast`：广播）  
| **MessageInfo**                          | 普通消息内容，详见MessageInfoModel                                                                         
| **Variables**                            | 模板参数
| **Receivers[]**                          | 外部收件人，详见ExternalReceiverModel
| **SendRules**                            | 发送规则，详见SendRuleModel    
| **ExtraProperties**                      | 扩展属性

#### 给外部用户发送模板消息

SendTemplateMessageByExternalAsync

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **ChannelCode**                          | 渠道编码/渠道 `ID`                                                                                  
| **ChannelType**                          | 渠道类型（`SMS`：短信，`Email`：邮箱，`WebsiteMessage`：站内信，`APP`：应用程序）                  
| **ReceiverType**                         | 发送类型（`Assign`：指定，`Broadcast`：广播）  
| **TemplateCode**                         | 模板编码/模板 `ID`
| **Variables**                            | 模板参数，发送模板消息时支持
| **Receivers[]**                          | 外部收件人，详见ExternalReceiverModel
| **SendRules**                            | 发送规则，详见SendRuleModel    
| **ExtraProperties**                      | 扩展属性

#### 普通消息内容

MessageInfoModel

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **Title**                                | 消息标题                                                                            
| **Content**                              | 消息内容
| **IsJump**                               | 是否跳转
| **JumpUrl**                              | 跳转地址
| **ExtraProperties**                      | 扩展属性，发送App消息时用于透传内容，发送站内信消息时可以指定Tag，支持根据Tag检索站内信

#### 发送规则

SendRuleModel

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **IsCustom**                             | 开启发送规则
| **CronExpression**                       | 使用Cron来实现分批发送或定时发送
| **SendingCount**                         | 每次要发送的条数，0即不限制

#### 内部收件人

InternalReceiverModel

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **SubjectId**                            | 用户 `ID`，具体内容通过 `MessageTaskReceiverTypes` 决定                                      
| **Type**                                 | 接收人类型（`User`：用户，`Organization`：组织架构，`Role`：角色，`Team`：团队，`Group`：收件人组）
| **Variables**                            | 收件人模板参数，优先级高于模板参数`Variables`


#### 外部收件人

ExternalReceiverModel

| 字段                                     | 描述                                                                                                
|------------------------------------------|-----------------------------------------------------------------------------------------------------
| **ChannelUserIdentity**                 | 渠道用户标识，`ChannelType` 为 `Email` 时填邮箱地址，`ChannelType` 为 `SMS` 时填手机号（具体内容通过 `ChannelType` 决定）  
| **Variables**                           | 收件人模板参数，优先级高于模板参数`Variables`

### SignalR 发送检查

站内信广播模式下通过 `SignalR` 发送检查通知，客户端接收后需要主动调用SDK的检查方法才会生成当前用户的站内信数据

```csharp Program.cs
var hubConnection = new HubConnectionBuilder()
    .WithUrl(NavigationManager.ToAbsoluteUri($"{McApiOptions.BaseAddress}/signalr-hubs/notifications"), options=>
    {
        options.AccessTokenProvider = async () =>
        {
            string? accessToken = string.Empty;
            if (httpContextAccessor.HttpContext != null)
            {
                accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            }
            return accessToken;
        };
    })
.Build();

await hubConnection.StartAsync();

//订阅检查通知
hubConnection?.On(SignalRMethodConsts.CHECK_NOTIFICATION, async () =>
{
    //广播下检查生成当前用户的站内信数据
    await McClient.WebsiteMessageService.CheckAsync();
});
```

### 绑定App ClientId

给用户推送App消息前需要绑定 `ClientId`

```csharp
[RoutePattern(HttpMethod = "Post")]
public async Task BindClientIdAsync([[FromServices] IMcClient mcClient, string clientId)
{
    var bindClientIdModel = new BindClientIdModel
    {
        ChannelCode = "渠道代码",
        ClientId = clientId,//个推cid或极光regId
    };
    await mcClient.MessageTaskService.BindClientIdAsync(bindClientIdModel);
}
```
