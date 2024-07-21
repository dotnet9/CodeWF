# 后台任务 - Hangfire

## 概述

基于 [Hangfire](https://www.hangfire.io/) 实现的后台任务，支持一次性任务和周期性任务

## 使用

1. 安装 `Masa.Contrib.Extensions.BackgroundJobs.Hangfire`

   ```shell 终端
   dotnet add package Masa.Contrib.Extensions.BackgroundJobs.Hangfire
   ```

2. 安装 `Hangfire.SqlServer`

   ```shell 终端
   dotnet add package Hangfire.SqlServer
   ```

   > 查看其它 [Storage](https://www.hangfire.io/extensions.html#storages)

3. 注册后台任务，并使用 [SqlServer](https://docs.hangfire.io/en/latest/configuration/using-sql-server.html) 数据库

   ```
   var services = new ServiceCollection();
   builder.Services.AddBackgroundJob(options =>
   {
       options.UseHangfire(configuration =>
       {
           configuration.UseSqlServerStorage("server=localhost;uid=sa;pwd=P@ssw0rd;database=hangfire");
       });
   });
   ```

4. 新建 `注册用户Dto`，用于传递参数

   ```csharp
   public class RegisterUserDto
   {
       public string Name { get; set; }
   }
   ```

5. 新增 `RegisterUserBackgroundJob`（注册用户处理程序）

   ```csharp
   using BackgroundJobsDemo.Dto;
   using Masa.BuildingBlocks.Extensions.BackgroundJobs;
   
   namespace BackgroundJobsDemo.Infrastructure;
   
   public class RegisterUserBackgroundJob : BackgroundJobBase<RegisterUserDto>
   {
       public RegisterUserBackgroundJob(ILogger<BackgroundJobBase<RegisterUserDto>>? logger) : base(logger)
       {
       }
   
       protected override Task ExecutingAsync(RegisterUserDto args)
       {
           Logger?.LogInformation("Execute registered account：{Name}", args.Name);
           return Task.CompletedTask;
       }
   }
   ```

6. 添加后台任务

   ```csharp Services/UserService.cs
   using BackgroundJobsDemo.Dto;
   using Masa.BuildingBlocks.Extensions.BackgroundJobs;
   
   namespace BackgroundJobsDemo.Services;
   
   public class UserService : ServiceBase
   {
       public Task AddAsync()
       {
           var registerUser = new RegisterUserDto()
           {
               Name = "masa"
           };
           return BackgroundJobManager.EnqueueAsync(registerUser, TimeSpan.FromSeconds(3));//Execute the task after 3s
       }
   }
   ```
