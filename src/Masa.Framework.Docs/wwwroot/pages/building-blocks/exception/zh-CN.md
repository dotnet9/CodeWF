# 异常处理

## 概述

为 `Web` 应用程序提供处理异常的模型，并提供了基于中间件的全局异常处理和针对 `MVC` 使用的异常过滤器来处理异常信息，在我们实际开发过程中，全局异常中间件与异常过滤器二选一使用即可

## 功能

* [支持 I18n](/framework/building-blocks/globalization/i18n)
* [支持自定义异常处理](#高阶用法)
* [自定义异常类型](#自定义异常处理)

## 使用

全局异常中间件与全局异常过滤器是两种处理异常的手段，它们的执行顺序是不同的，详细可查看 [ASP.NET Core 中间件](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware)、[ASP.NET Core 中的筛选器](https://learn.microsoft.com/zh-cn/aspnet/core/mvc/controllers/filters)

默认支持 `i18n` ，当服务使用 `i18n` 后，异常信息会根据请求文化（`Culture`）信息转换为对应的语言

1. 安装 `Masa.Contrib.Exceptions`

   ```shell 终端
   dotnet add package Masa.Contrib.Exceptions
   ```

2. 使用全局异常中间件/全局异常过滤器

   :::: code-group
   ::: code-group-item 全局异常中间件

   ```csharp Program.cs
   app.UseMasaExceptionHandler();
   ```
   :::
   ::: code-group-item 全局异常过滤器

   ```csharp Program.cs l:3
   builder.Services
       .AddMvc()
       .AddMasaExceptionHandler();
   ```
   :::
   ::::

   > 中间件、过滤器二选一使用即可

## 配置

### 异常类型与日志等级

在全局异常中间件或异常过滤器中，异常类型与日志的默认映射关系：

* UserFriendlyException：Information
* 非UserFriendlyException异常：Error

如果我希望异常类型为 `UserFriendlyException`（友好异常）不记录日志

```csharp
builder.Services.Configure<MasaExceptionLogRelationOptions>(options =>
{
    options.MapLogLevel<UserFriendlyException>(LogLevel.None);
});
```

> 通过配置异常类型与日志等级，它将更改全局异常类型与日志等级的默认关系，但如果抛出异常时指定了日志等级，则当前异常不受默认关系影响，比如:

```csharp
throw new MasaException("Custom exception error, the current log level is Warning.", LogLevel.Warning);
```

## 高阶用法

### 自定义异常处理

#### 中间件

:::: code-group
::: code-group-item 手动指定异常处理
```csharp Program.cs l:4-8
app.UseMasaExceptionHandler(options =>
{
    //处理自定义异常
    options.ExceptionHandler = context =>
    {
        if (context.Exception is ArgumentNullException ex)
            context.ToResult(ex.Message, 299);
    };
});
```
:::
::: code-group-item 注册自定义异常处理程序
```csharp l:1-18,22
public class ExceptionHandler : IMasaExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;
    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public void OnException(MasaExceptionContext context)
    {
        if (context.Exception is ArgumentNullException)
        {
            _logger.LogWarning(context.Message);
            context.ToResult(context.Exception.Message, 299);
        }
    }
}
builder.Services.AddSingleton<ExceptionHandler>();

var app = builder.Build();

app.UseMasaExceptionHandler();
```
:::
::: code-group-item 使用自定义异常处理程序
```csharp l:1-10,12-15
public class ExceptionHandler : IMasaExceptionHandler
{
    public void OnException(MasaExceptionContext context)
    {
        if (context.Exception is ArgumentNullException)
        {
            context.ToResult(context.Exception.Message, 299);
        }
    }
}

app.UseMasaExceptionHandler(option =>
{
    option.UseExceptionHandler<ExceptionHandler>();
});
```
:::
::::

> 方案2：（注册自定义异常处理程序）支持通过`DI`获取
> 
> 方案3：（使用自定义异常处理程序）需确保有无参构造函数，且不支持从`DI`获取服务

#### 异常过滤器

:::: code-group
::: code-group-item 手动指定异常处理
```csharp Program.cs l:3-10
builder.Services
    .AddMvc()
    .AddMasaExceptionHandler(options =>
    {
        options.ExceptionHandler = context =>
        {
            if (context.Exception is ArgumentNullException ex)
                context.ToResult(ex.Message, 299);
        };
    });
```
:::
::: code-group-item 注册自定义异常处理程序
```csharp l:1-17,19,23
public class ExceptionHandler : IMasaExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;
    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public void OnException(MasaExceptionContext context)
    {
        if (context.Exception is ArgumentNullException)
        {
            _logger.LogWarning(context.Message);
            context.ToResult(context.Exception.Message, 299);
        }
    }
}

builder.Services.AddSingleton<ExceptionHandler>();

builder.Services
  .AddMvc()
  .AddMasaExceptionHandler();
```
:::
::: code-group-item 使用自定义异常处理程序
```csharp l:1-10,14,17
public class ExceptionHandler : IMasaExceptionHandler
{
    public void OnException(MasaExceptionContext context)
    {
        if (context.Exception is ArgumentNullException)
        {
            context.ToResult(context.Exception.Message, 299);
        }
    }
}

builder.Services
  .AddMvc()
  .AddMasaExceptionHandler(options =>
  {
      options.UseExceptionHandler<ExceptionHandler>();
  });
```
:::
::::

> 方案2：（注册自定义异常处理程序）支持通过`DI`获取
>
> 方案3：（使用自定义异常处理程序）需确保有无参构造函数，且不支持从`DI`获取服务

### 异常与Http状态码

`MASA Framework` 提供的了几种常用的异常类型，当 `API` 服务对外抛出以下异常类型时，服务端将响应与之对应的 `Http` 状态码：

|  异常类型   | 描述  |  HttpStatusCode  |
|  ----  | ----  | ----  |
| UserFriendlyException  | 用户友好异常 | 299 |
| MasaValidatorException  | 验证异常 | 298 |
| MasaArgumentException  | 参数异常 | 500 |
| MasaException  | 内部服务错误 | 500 |

其中 `HttpStatusCode` 为298是验证异常，存在固定格式的响应信息

``` http
Validation failed: 
-- {Name}: {Message} Severity: {ValidationLevel}
-- {Name2}: {Message2} Severity: {ValidationLevel}
```

> 配合 [`Masa Blazor`](https://docs.masastack.com/blazor/introduction/why-masa-blazor) 使用，可以提供更友好的表单验证

### 多语言

异常与 [I18n](/framework/building-blocks/globalization/overview) 结合使用，会有更好的使用体验

1. 安装 `Masa.Contrib.Globalization.I18n.AspNetCore`

   ```shell 终端
   dotnet add package Masa.Contrib.Globalization.I18n.AspNetCore
   ```

2. 安装 `exception`

   ```shell 终端
   dotnet add package Masa.Contrib.Exceptions
   ```

3. 注册 `i18n` 并使用 全局异常中间件

   ```csharp Program.cs l:3,9,11-17
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddI18n();
   
   var app = builder.Build();
   
   app.UseHttpsRedirection();
   
   app.UseI18n();
   
   app.UseMasaExceptionHandler(exceptionHandlerOptions =>
   {
       exceptionHandlerOptions.ExceptionHandler = exceptionContext =>
       {
           
       };
   });
   
   app.Run();
   ```

4. 新建 多语言资源文件

   :::: code-group
   ::: code-group-item en-US.json

   ```json Resources/I18n/en-US.json
   {
     "Exception": {
       "NotSupport": "Unsupported API"
     },
     "Tip": {
       "VerificationSucceeded": "Parameter verification succeeded"
     }
   }
   ```
   :::
   ::: code-group-item zh-CN.json

   ```json Resources/I18n/zh-CN.json
   {
     "Exception": {
       "NotSupport": "不支持的API"
     },
     "Tip": {
       "VerificationSucceeded": "参数校验成功"
     }
   }
   ```
   :::
   ::: code-group-item supportedCultures.json

   ```json Resources/I18n/supportedCultures.json
   [
       {
           "Culture":"zh-CN",
           "DisplayName":"中文简体",
           "Icon": "{Replace-Your-Icon}"
       },
       {
           "Culture":"en-US",
           "DisplayName":"English (United States)",
           "Icon": "{Replace-Your-Icon}"
       }
   ]
   ```

   :::
   ::::

5. 参数异常 + 友好异常

   ```csharp Program.cs l:3-4,9
   app.MapGet("/parameter/verify", (int page) =>
   {
       MasaArgumentException.ThrowIfLessThan(page, 1);
       return I18n.T("Tip.VerificationSucceeded");
   });
   
   app.MapGet("/exception", () =>
   {
       throw new UserFriendlyException(errorCode: "Exception.NotSupport");
   });
   ```

## 源码解读

### MasaException

基于 `Exception` 的扩展类，是 `MASA Framework` 提供的异常基类，对外抛出 `HttpStatusCode` 为`500`的错误码，并将其 `Message` 作为响应内容输出

* ErrorCode：错误码，针对 `ErrorCode` 不为 `null` 且不等于空字符，在开启了[多语言](/framework/building-blocks/globalization/overview)后，可以通过 `GetLocalizedMessage` 方法获取当前语言的错误信息

### UserFriendlyException

基于 `MasaException` 的扩展类，是 `MASA Framework` 提供的用户友好异常类，对外抛出 `HttpStatusCode` 为`299`的错误码，并将其 `Message` 作为响应内容输出

### MasaArgumentException

基于 `MasaException` 的扩展类，是 `MASA Framework` 提供的参数异常类，默认对外抛出 `HttpStatusCode` 为`500`的错误码，并将其 `Message` 作为响应内容输出，提供了以下方法

* ThrowIfNullOrEmptyCollection：参数为 `Null` 或者空集合时抛出异常
* ThrowIfNull：参数为 `Null` 时抛出异常
* ThrowIfNullOrEmpty：参数为 `Null` 或空字符串时抛出异常
* ThrowIfNullOrWhiteSpace：参数为 `Null` 或空白字符时抛出异常
* ThrowIfGreaterThan：参数大于 `{value}` 时抛出异常
* ThrowIfGreaterThanOrEqual：参数大于等于 `{value}` 时抛出异常
* ThrowIfLessThan：参数小于 `{value}` 时抛出异常
* ThrowIfLessThanOrEqual：参数小于等于 `{value}` 时抛出异常
* ThrowIfOutOfRange：参数不在指定范围之间时抛出异常 (\< minValue & \> maxValue)
* ThrowIfContain：参数包含指定字符串时抛出异常
* ThrowIf：条件满足时抛出异常

### MasaValidatorException

基于 MasaArgumentException 的扩展类，是 MASA Framework 提供的验证异常类，默认对外抛出 `HttpStatusCode` 为`298`的错误码，对外输出内容为固定格式：

``` http
"Validation failed: 
-- {Name}: {Message1} Severity: {ValidationLevel}
-- {Name2}: {Message2} Severity: {ValidationLevel}"
```

> 与 [MasaBlazor](/blazor/introduction/why-masa-blazor) 结合使用可以提供更好的表单验证效果
