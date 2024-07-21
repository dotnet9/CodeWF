# 后台任务 - 常见问题

## 概述

记录了使用 **后台任务** 可能遇到的问题以及问题应该如何解决

## 公共

1.  使用`Controller`的项目添加后台任务成功，但未能成功执行？

如果使用的不是 `MinimalAPIs` 方案，则需要通过 `DI` 获取到 `IBackgroundJobManager` 服务使用，或者将 项目 `RootServiceProvider` 赋值给 `MasaApp`，以 **内存**后台任务为例

* 方案1：通过 IBackgroundJobManager 使用

  ```csharp Controllers/UserController
  using BackgroundJobsDemo.Dto;
  using Masa.BuildingBlocks.Extensions.BackgroundJobs;
  using Microsoft.AspNetCore.Mvc;
  
  namespace BackgroundJobsDemo.Controllers;
  
  [Route("[controller]/[action]")]
  public class UserController: ControllerBase
  {
      private readonly IBackgroundJobManager _backgroundJobManager;
      public UserController(IBackgroundJobManager backgroundJobManager)
      {
          _backgroundJobManager = backgroundJobManager;
      }
      
      [HttpPost]
      public Task AddAsync()
      {
          var registerUser = new RegisterUserDto()
          {
              Name = "masa"
          };
          return _backgroundJobManager.EnqueueAsync(registerUser, TimeSpan.FromSeconds(3));//Execute the task after 3s
      }
  }
  ```

* 方案2：通过静态方法使用

  :::: code-group
  ::: code-group-item 设置 RootServiceProvider
  ```csharp Program.cs
  using Masa.BuildingBlocks.Data;
  using Masa.BuildingBlocks.Extensions.BackgroundJobs;
  
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddBackgroundJob(options =>
  {
      options.UseInMemoryDatabase();
  });
  
  builder.Services.AddControllers();
  
  var app = builder.Build();
  
  MasaApp.Build(app.Services);//Set RootServiceProvider
  
  app.UseHttpsRedirection();
  
  app.MapControllers();
  
  app.Run();
  ```
  :::
  ::: code-group-item 通过静态方法使用
  ```
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
  :::
  ::::