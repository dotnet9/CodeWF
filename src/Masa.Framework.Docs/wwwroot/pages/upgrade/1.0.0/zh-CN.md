# 1.0.0 升级指南

为避免歧义，减少学习成本，在 **1.0.0** 中我们对项目中的一些接口/类名进行了重命名调整，这导致出现一些破坏性修改

## 破坏性修改

1. <font color=Red>Middleware</font>  ->  <font color=Red>EventMiddleware</font>， 受影响的接口以及类为:

    * `Masa.BuildingBlocks.Dispatcher.Events`
        * `IMiddleware<in TEvent>` → `IEventMiddleware<in TEvent>`
        * `Middleware<in TEvent>` → `EventMiddleware<in TEvent>`
    * `Masa.Contrib.Dispatcher.Events.FluentValidation`
    	* `ValidatorMiddleware<TEvent>` →  `ValidatorEventMiddleware<TEvent>`
    * `Masa.Contrib.Dispatcher.Events`
    	* `TransactionMiddleware<TEvent>` → `TransactionEventMiddleware<TEvent>`
    * `Masa.Contrib.Isolation`
    	* `IsolationMiddleware<TEvent>` → `IsolationEventMiddleware<TEvent>`

    > 由于事件总线提供的中间件与微软提供的中间件名称冲突，我们在原类名的基础上增加Event。

2.  `FluentValidation` 扩展验证 (包名: `Masa.Utils.Extensions.Validations.FluentValidation`)
   
    1. <font color=Red>FluentValidation扩展验证器</font>都修改为<font color=Red>Null</font>值默认<font color=Red>不</font>再进行<font color=Red>校验</font>。[PR #485](https://github.com/masastack/MASA.Framework/pull/485)，必填的字段，需要加入`NotNull`校验
    
        * `RuleFor(staff => staff.PhoneNumber).Phone();`需要修改为 `RuleFor(staff => staff.PhoneNumber).NotNull().Phone();`。
        * `RuleFor(staff => staff.IdCard).IdCard();`需要修改为 `RuleFor(staff => staff.IdCard).NotNull().IdCard();`。

      > MASA Framework 定义的 `Validator` 扩展默认逻辑与 `FluentValidation` 官方的内置 `Validator` 逻辑不一致，为了保持一致，验证器扩展默认不对 `Null` 值进行校验，受影响的`PhoneValidator`、`IdCardValidator`
   
    2. 预定义的正则表达式修改，<font color=Red>不允许空字符串通过校验</font>，受影响的验证器扩展为:
      
       * Chinese （中文校验）
       * Number  （数字校验）
       * Letter  （字母校验）
       * Identity
       * LowerLetter  （小写字母校验）
       * UpperLetter  （大写字母校验）
       * LetterNumber （小写字母 + 数字）
       * ChineseLetterNumber （中文 + 小写字母 + 数字）
       * ChineseLetter （中文 + 小写字母）
       * ChineseLetterNumberUnderline （中文 + 小写字母 + 数字 + 下划线）
       * ChineseLetterUnderline （中文 + 小写字母 + 下划线）
       * Url （Url地址）
       * Email （邮箱）
       * Password （默认密码验证器）
       * Port （端口）

3. `MasaDbContext` <font color=Red>查询默认不跟踪</font>

    ```csharp Infrastructure/CustomDbContext.cs
    public class CustomDbContext : MasaDbContext<CustomDbContext>
    {
         protected MasaDbContext(MasaDbContextOptions<CustomDbContext> options) : base(options)
         {
             ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
         }
    }
    ```
   
4. `MasaDbContext` 注册时**不再支持传入用户 ID 类型**

* 以用户id为int为例：

  :::: code-group
  ::: code-group-item 调整前
  ```csharp Program.cs
  builder.Services.AddMasaDbContext<CustomDbContext, int>(options =>
  {
      options.UseSqlServer("更换SqlServer数据库地址");
  });
  ```
  :::
  ::: code-group-item 调整后
  ```csharp Program.cs
  builder.Services.Configure<AuditEntityOptions>(options => options.UserIdType = typeof(int));
  
  builder.Services.AddMasaDbContext<CustomDbContext>(options =>
  {
      options.UseSqlServer("更换SqlServer数据库地址");
  });
  ```
  :::
  ::::
  
  > 在一个系统中，用户 `ID` 不可能出现多种用户类型的情况，**未配置用户  ID 类型，默认:  Guid **

5. 工作单元注册时**不再支持传入用户 ID 类型**

* 假如用户 `ID` 为 `Int`：

  :::: code-group
  ::: code-group-item 调整前
  
  ```csharp Program.cs l:2,7
  builder.Services
      .AddMasaDbContext<CustomDbContext, int>(optionsBuilder => optionsBuilder.UseSqlServer("更换SqlServer数据库地址"))
      .AddDomainEventBus(options =>
      {
          options.UseIntegrationEventBus(dispatcherOptions => dispatcherOptions.UseDapr().UseEventLog<CustomDbContext>())
              .UseEventBus(eventBuilder => eventBuilder.UseMiddleware(typeof(ValidatorMiddleware<>)))
              .UseUoW<CustomDbContext, int>()
              .UseRepository<CustomDbContext>();
      });
  ```
  :::
  ::: code-group-item 调整后
  
  ```csharp Program.cs l:1,4,9
  builder.Services.Configure<AuditEntityOptions>(options => options.UserIdType = typeof(int));
  
  builder.Services
      .AddMasaDbContext<CustomDbContext>(optionsBuilder => optionsBuilder.UseSqlServer("更换 SqlServer 数据库地址"))
      .AddDomainEventBus(options =>
      {
          options.UseIntegrationEventBus(dispatcherOptions => dispatcherOptions.UseDapr().UseEventLog<CustomDbContext>())
              .UseEventBus(eventBuilder => eventBuilder.UseMiddleware(typeof(ValidatorMiddleware<>)))
              .UseUoW<CustomDbContext>()
              .UseRepository<CustomDbContext>();
      });
  ```
  :::
  ::::

6. 隔离性调整

    * <font color=Red>删除 **Masa.Contrib.Isolation.UoW.EFCore** </font>
    * 用法调整

        :::: code-group
        ::: code-group-item 调整前
        
        ```csharp Program.cs l:3,12
        var builder = WebApplication.CreateBuilder(args);
        builder.Services
            .AddMasaDbContext<CustomDbContext>(optionsBuilder => optionsBuilder.UseSqlServer("更换 SqlServer 数据库地址"))
            .AddDomainEventBus(dispatcherOptions =>
            {
                dispatcherOptions.UseIntegrationEventBus<IntegrationEventLogService>(options => options.UseDapr().UseEventLog<CustomDbContext>())
                    .UseEventBus(eventBusBuilder =>
                    {
                        eventBusBuilder.UseMiddleware(typeof(DisabledCommandMiddleware<>));
                        eventBusBuilder.UseMiddleware(typeof(ValidatorMiddleware<>));
                    })
                    .UseIsolationUoW<CustomDbContext>(isolationBuilder => isolationBuilder.UseMultiTenant())
                    .UseRepository<CustomDbContext>();
            });
        
        var app = builder.Build();
        
        app.UseIsolation();
        
        app.Run();
        ```
        :::
        ::: code-group-item 调整后
        
        ```csharp Program.cs l:5,7
        var builder = WebApplication.CreateBuilder(args);
        builder.Services
            .AddMasaDbContext<CustomDbContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServer("更换 SqlServer 数据库地址"));
            })
        	.AddIsolation(isolationBuilder => isolationBuilder.UseMultiTenant());//添加隔离性（使用多租户）
          
      var app = builder.Build();
        
        app.UseIsolation();
        
        app.Run();
      ```
        :::
        ::::
    
    > 调整后的隔离性与 `DDD` 无关，通过引用 `Masa.Contrib.Isolation.MultiTenant`、`Masa.Contrib.Data.EFCore.SqlServer` 即可实现多租户

7. 取消 `Masa.Contrib.Exceptions` 对 `Masa.Contrib.Globalization.I18n.AspNetCore` 的强依赖

   使用 `Masa.Contrib.Exceptions` 并非必须使用 `I18n` ，它们之间不属于强依赖，如果需要使用 `I18n` 需要手动引用 `Masa.Contrib.Globalization.I18n.AspNetCore` 进行安装

## 功能

1. `FluentValidation` 扩展验证 (包名: `Masa.Utils.Extensions.Validations.FluentValidation`)

   * 新增抽象类 `Masa.Utils.Extensions.Validations.FluentValidation.MasaAbstractValidator<T>`，提供 `WhenNotEmpty` 方法

      为了支持业务系统的可选值的校验，新增了抽象类 `MasaAbstractValidator<T>`，提供 `WhenNotEmpty` 扩展方法，方便用户对一些可选的验证进行处理。

      举例: 如果只在当` Phone` 有值的时候进行校验，可以按以下的方式进行调用

       ```csharp
       public class RegisterUser
       {
           public string Account { get; set; }
      
           public string Password { get; set; }
       }
      
       public class RegisterUserValidator : MasaAbstractValidator<RegisterUser>
       {
           public RegisterUserValidator()
           {
               // 以前的调用方式
               //When(r => !string.IsNullOrEmpty(r.Phone), () => RuleFor(r => r.Phone).Phone());
      
               // WhenNotEmpty 的调用示例,任选一种
               //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, new PhoneValidator<RegisterUser>());
               //_ = WhenNotEmpty(r => r.Phone, new PhoneValidator<RegisterUser>());
               //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, rule => rule.Phone());
               _ = WhenNotEmpty(r => r.Phone, rule => rule.Phone());
           }
       }
       ```

2. <font color=Red>MasaDbContext</font> 新增支持 <font color=Red>无参构造函数</font>

    升级前:

    :::: code-group
    ::: code-group-item 创建数据上下文

    ```csharp Infrastructure/CustomDbContext.cs
    public class CustomDbContext : MasaDbContext<CustomDbContext>
    {
        public CustomDbContext(MasaDbContextOptions<CustomDbContext> options) : base(options)
        {
        }
    }
    ```
    :::
    ::: code-group-item 注册自定义上下文

    ```csharp Program.cs
    var services = new ServiceCollection();
    services.AddMasaDbContext<CustomDbContext>(builder => builder.UseSqlite("data source=customDbContext"));
    ```
    :::
    ::::

    升级后:

    :::: code-group
    ::: code-group-item 创建数据上下文
    ```csharp Infrastructure/CustomDbContext.cs
      public class CustomDbContext : MasaDbContext<CustomDbContext>
      {
          protected override void OnConfiguring(MasaDbContextOptionsBuilder optionsBuilder)
          {
              optionsBuilder.UseSqlite("data source=customDbContext");
          }
      }
    ```
    :::
    ::: code-group-item 注册自定义上下文
    ```csharp Program.cs
    var services = new ServiceCollection();
    services.AddMasaDbContext<CustomDbContext>();
    ```
    :::
    ::::

    > 相比注册 MasaDbContext 时使用数据库，新语法会有一些特别要注意的地方，详细可查看[文档](/framework/building-blocks/data/orm-efcore)

3. 新增 <font color=Red>支持后台任务</font>，支持以下实现 **内存数据库**、**Hangfire**

    * 以内存数据库为例:

        :::: code-group
        ::: code-group-item 安装包
        
        ```shell 终端
        dotnet add package Masa.Contrib.Extensions.BackgroundJobs.Memory
        ```
        :::
        ::: code-group-item 注册后台任务并使用内存数据库
        ```csharp Program.cs
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddBackgroundJob(jobBuilder =>
        {
             jobBuilder.UseInMemoryDatabase();
        });
        var app = builder.Build();
        MasaApp.Build(app.Services);//托管RootServiceProvider，如果通过DI获取到IBackgroundJobManager服务使用可忽略
        ```
        :::
        ::: code-group-item 注册账户类
        ```csharp
        public class RegisterAccountParameter
        {
            public string Account { get; set; }
        }
        ```
        :::
        ::: code-group-item 注册账户处理程序
        ```csharp
        public class RegisterAccountBackgroundJob : IBackgroundJob<RegisterAccountParameter>
        {
            public Task ExecuteAsync(RegisterAccountParameter args)
            {
                Console.WriteLine("任务执行: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return Task.CompletedTask;
            }
        }
        ```
        :::
        ::: code-group-item 添加后台任务
        ```csharp Program.cs
        app.MapGet("register", () =>
        {
            Console.WriteLine("任务执行: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var registerAccount = new RegisterAccountParameter()
            {
                Account = "test" + DateTime.Now.ToLongDateString()
            };
            BackgroundJobManager.EnqueueAsync(registerAccount, TimeSpan.FromSeconds(5));//5秒后开始执行
        });
        ```
        :::
        ::::