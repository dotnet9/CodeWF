# 事件总线 - 进程内事件

## 概述

进程内事件总线允许服务发布和订阅进程内事件. 如果发布者和订阅者在同一个进程中运行，那么使用进程内事件总线是合适的，在本地事件总线中，我们支持了 UnitOfWork，除此之外，订阅方支持按照顺序执行，同时还支持 `Saga`

<div>
  <img alt="EventBus" src="https://cdn.masastack.com/framework/building-blocks/dispatcher/local-event/event-bus.png"/>
</div>

## 功能列表

* [编排](#编排): 支持Handler按顺序执行
* [中间件](#中间件): 支持自定义中间件
* [Saga](#Saga): 支持补偿机制，[详细](https://learn.microsoft.com/zh-cn/azure/architecture/reference-architectures/saga/saga)

## 使用

1. 安装 `Masa.Contrib.Dispatcher.Events`

   ```shell 终端
   dotnet add package Masa.Contrib.Dispatcher.Events
   ```

2. 注册 EventBus

   ```csharp Program.cs l:2
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddEventBus();
   ```

3. 新增 RegisterUserEvent 类

   ```csharp Application/User/RegisterUserEvent.cs l:1
   public record RegisterEvent : Event
   {
       public string Account { get; set; }
   
       public string Email { get; set; }
   
       public string Password { get; set; }
   }
   ```

4. 注册用户的处理程序

   ```csharp l:11-16
   public class UserHandler
   {
       private readonly ILogger<UserHandler>? _logger;
   
       public UserHandler(ILogger<UserHandler>? logger = null)
       {
           //todo: 根据需要可在构造函数中注入其它服务 (需支持从DI获取)
           _logger = logger;
       }
   
       [EventHandler]
       public void RegisterUser(RegisterUserEvent @event)
       {
           //todo: 编写注册用户业务
           _logger?.LogDebug("-----------{Message}-----------", "检测用户是否存在并注册用户");
       }
   }
   ```

5. 发布注册用户事件

   ```csharp Program.cs
   app.MapPost("/register", async (RegisterUserEvent @event, IEventBus eventBus) =>
   {
       await eventBus.PublishAsync(@event);
   });
   ```

## 高阶用法

### 编排

Handler 按照 Order 的值从小到大升序执行

```csharp
public class UserHandler
{
    private readonly ILogger<UserHandler>? _logger;

    public UserHandler(ILogger<UserHandler>? logger = null)
    {
        _logger = logger;
    }

    [EventHandler(Order = 1)]
    public void RegisterUser(RegisterUserEvent @event)
    {
        _logger?.LogDebug("-----------{Message}-----------", "检测用户是否存在并注册用户");
        //todo: 编写注册用户业务
    }

    [EventHandler(Order = 2)]
    public void SendAwardByRegister(RegisterUserEvent @event)
    {
        _logger?.LogDebug("-----------{Account} 注册成功 {Message}-----------", @event.Account, "发送注册奖励");
        //todo: 编写发送奖励等
    }

    [EventHandler(Order = 3)]
    public void SendNoticeByRegister(RegisterUserEvent @event)
    {
        _logger?.LogDebug("-----------{Account} 注册成功 {Message}-----------", @event.Account, "发送注册成功邮件");
        //todo: 编写发送注册通知等
    }
}
```

### 中间件

EventBus 的请求管道包含一系列请求委托，依次调用。 它们与 [ASP.NET Core中间件](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-7.0#create-a-middleware-pipeline-with-webapplication)有异曲同工之妙

![EventBus.png](https://s2.loli.net/2023/01/15/mT916WIDAkcPZFt.png)

每个委托均可在下一个委托前后执行操作，其中 [`TransactionEventMiddleware`](https://github.com/masastack/MASA.Framework/blob/0.6.0/src/Contrib/Dispatcher/Masa.Contrib.Dispatcher.Events/Internal/Middleware/TransactionEventMiddleware.cs) 是 EventBus 发布后第一个要进入的中间件（默认提供），并且它是不支持多次嵌套的。

> EventBus 支持嵌套发布事件，这意味着我们可以在 Handler 中重新发布一个新的 Event ，但对于不支持嵌套的中间件，其仅会在最外层进入时被触发一次

比如： 新增日志事件中间件，用于记录事件的日志信息

:::: code-group
::: code-group-item 1. 创建日志中间件

```csharp Infrastructure/Middlewares/LoggingEventMiddleware.cs
public class LoggingEventMiddleware<TEvent> : EventMiddleware<TEvent>
    where TEvent : IEvent
{
    private readonly ILogger<LoggingEventMiddleware<TEvent>> _logger;
    public LoggingEventMiddleware(ILogger<LoggingEventMiddleware<TEvent>> logger) => _logger = logger;

    public override async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
    {
        _logger.LogInformation("----- Handling command {CommandName} ({@Command})", @event.GetType().GetGenericTypeName(), @event);
        await next();
    }
}
```
:::
::: code-group-item 2. 使用验证中间件

```csharp l:2
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEventBus(eventBusBuilder => eventBusBuilder.UseMiddleware(typeof(LoggingEventMiddleware<>)));
```
:::
::::

### Saga 模式

一种业务补偿模式，将一个请求的处理程序划分为不同的步骤执行，每个步骤都对应一个取消方法，当所有步骤成功后，提交数据并保存，如果出现错误，则按照执行顺序反向执行取消动作，默认情况下，当出现异常时，将执行

<div><img alt="saga" src="https://s2.loli.net/2023/02/14/cnEWvwpCXoGZiqt.png"/></div>

例如: 新增加`CancelSendAwardByRegister`方法用于补偿发送失败奖励出错的问题. 执行流程为:

**RegisterUser -> SendAwardByRegister -> CancelSendAwardByRegister**

```csharp
public class UserHandler
{
    private readonly ILogger<UserHandler>? _logger;

    public UserHandler(ILogger<UserHandler>? logger = null)
    {
        _logger = logger;
    }

    [EventHandler(1)]
    public void RegisterUser(RegisterUserEvent @event)
    {
        _logger?.LogDebug("-----------{Message}-----------", "检测用户是否存在并注册用户");
        //todo: 编写注册用户业务
    }

    [EventHandler(2)]
    public void SendAwardByRegister(RegisterUserEvent @event)
    {
        _logger?.LogDebug("-----------{Account} 注册成功 {Message}-----------", @event.Account, "发送注册奖励");
        //todo: 编写发送奖励等

        throw new Exception("发送奖励出错");
    }

    [EventHandler(1, IsCancel = true)]
    public void CancelSendAwardByRegister(RegisterUserEvent @event)
    {
        _logger?.LogDebug("-----------{Account} 注册成功，发放奖励失败 {Message}-----------", @event.Account, "发放奖励补偿");
    }

    [EventHandler(3)]
    public void SendNoticeByRegister(RegisterUserEvent @event)
    {
        _logger?.LogDebug("-----------{Account} 注册成功 {Message}-----------", @event.Account, "发送注册成功邮件");
        //todo: 编写发送注册通知等
    }
}
```

> SendAwardByRegister 出错后，将执行的补偿动作的顺序为: 1 -> 0，如果希望执行与其 Order 一致的补偿动作，则需要更改 `FailureLevels` 属性的值，详细可查看[特性](#特性)

### 事务

EventBus 支持事务，当配合 UnitOfWork 使用时，当出现异常时会自动回滚，避免脏数据入库

### 特性

通过在方法上增加 `EventHandler` 来标记当前方法是当前事件 (当前方法的参数中继承 IEvent 的类)的 Handler

* Order：执行顺序，默认：int.MaxValue
* FailureLevels：失败级别，默认：Throw
  * Throw：发生异常后，依次执行Order小于当前Handler的Order的补偿动作，比如：Handler顺序为 1、2、3，CancelHandler为 1、2、3，如果执行 Handler3 异常，则依次执行 2、1
  * ThrowAndCancel：发生异常后，依次执行Order小于等于当前Handler的Order的补偿动作，比如：Handler顺序为 1、2、3，CancelHandler为 1、2、3，如果执行 Handler3 异常，则依次执行 3、2、1
  * Ignore：忽略当前异常（不执行取消动作），继续执行其他Handler
* EnableRetry：出现异常后是否重试，默认：false
* RetryTimes：出现异常后的最大重试次数，默认：3 (EnableRetry为true生效)
* IsCancel：当前Handler是否是补偿动作，默认：false

### 接口约束

通过 `IEventHandler<TEvent>`  或  `ISagaEventHandler<TEvent>` 来标记当前事件的 Handler ， `IEventHandler` 与 `ISagaEventHandler` 的区别在于一个不存在补偿动作，一个存在补偿动作

* HandleAsync(TEvent @event)：提供事件的 Handler
* CancelAsync(TEvent @event)：提供事件的补偿 Handler

接口约束与 `EventHandler` 特性的功能类似，唯一的区别在于写法，接口约束目前仅支持最基本的 Handler 以及补偿 Handler ，我们更推荐通过 `EventHandler`特性来使用 EventBus，两种任选其一即可

## 扩展

基于进程内事件总线，我们提供了基于 `FluentValidation` 的事件验证中间件，通过它可以使得我们的事件校验变得更加优雅

### 事件验证中间件

1. 安装 `Masa.Contrib.Dispatcher.Events.FluentValidation`、`FluentValidation.AspNetCore`

   ```shell 终端
   dotnet add package Masa.Contrib.Dispatcher.Events.FluentValidation
   dotnet add package FluentValidation.AspNetCore
   ```

2. 指定进程内事件使用 FluentValidation 的中间件

   ```csharp Program.cs l:3-4
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddValidatorsFromAssembly(Assembly.GetEntryAssembly());
   builder.Services.AddEventBus(eventBusBuilder => eventBusBuilder.UseMiddleware(typeof(ValidatorEventMiddleware<>)));
   ```

3. 创建事件的校验处理，例如：

   ```csharp
   public class RegisterEventValidator : AbstractValidator<RegisterEvent>
   {
       public RegisterEventValidator()
       {
           RuleFor(e => e.Account).NotNull().WithMessage("用户名不能为空");
           RuleFor(e => e.Email).NotNull().WithMessage("邮箱不能为空");
           RuleFor(e => e.Password)
               .NotNull().WithMessage("密码不能为空")
               .MinimumLength(6)
               .WithMessage("密码必须大于6位")
               .MaximumLength(20)
               .WithMessage("密码必须小于20位");
       }
   }
   ```

   > 不满足事件校验规则的请求会对外抛出指定内容的 `MasaValidatorException` 异常

## 性能测试

与市面上使用较多的 `MeidatR`作了对比，结果如下图所示:

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1023 (21H1/May2021Update)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.100-preview.4.22252.9
  [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT DEBUG
  Job-MHJZJL : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT

Runtime=.NET 6.0  IterationCount=100  RunStrategy=ColdStart

|                         Method |      Mean |     Error |      StdDev |   Median |      Min |         Max |
|------------------------------- |----------:|----------:|------------:|---------:|---------:|------------:|
| AddShoppingCartByEventBusAsync | 124.80 us | 346.93 us | 1,022.94 us | 8.650 us | 6.500 us | 10,202.4 us |
|  AddShoppingCartByMediatRAsync | 110.57 us | 306.47 us |   903.64 us | 7.500 us | 5.300 us |  9,000.1 us |

根据性能测试我们发现，EventBus 与 MediatR 性能差距很小，但 EventBus 提供的功能却要强大的多
