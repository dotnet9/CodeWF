# 后台任务 - 内存

## 概述

基于内存实现的后台任务，项目重启后任务丢失

## 使用

1. 安装 `Masa.Contrib.Extensions.BackgroundJobs.Memory`

   ```shell 终端
   dotnet add package Masa.Contrib.Extensions.BackgroundJobs.Memory
   ```

2. 注册内存后台任务

   ```csharp Program.cs
   var services = new ServiceCollection();
   builder.Services.AddBackgroundJob(options =>
   {
       options.UseInMemoryDatabase();
   });
   ```

3. 新建 `注册用户Dto`，用于传递参数

   ```csharp
   public class RegisterUserDto
   {
       public string Name { get; set; }
   }
   ```

4. 新增 `RegisterUserBackgroundJob`（注册用户处理程序）

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

5. 添加后台任务

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