# 快速入门 - Web应用程序

本章是一个使用 **MASA Framework** 创建 Web 程序的快速入门教程，我们会使用 **MASA Framework** 创建一个简单的待办事项应用程序。以下是程序最终运行效果：

![运行效果图](https://cdn.masastack.com/framework/getting-started/web-project/result.png)

## 下载源码

* [Masa.Framework.TodoApp](https://github.com/masalabs/Masa.Framework.TodoApp)

## 项目依赖

* 开发工具：Visual Studio 2022 及更高版本或其它支持 [`.NET 6.0`](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) `.NET SDK` 版本的开发工具
* .NET 版本：6.0 +

## 安装 MASA Template

本教程将会使用 `MASA Template` 创建 TodoApp 解决方案。我们先打开 `cmd` 运行下面的命令安装模板

```shell 终端
dotnet new install MASA.Template
```

## 创建项目

在目录下使用 cmd 运行以下命令，它将会创建整个 TodoApp 解决方案。在这里我们将使用 [MASA.Blazor](https://docs.masastack.com/blazor/getting-started/installation) 来完成我们UI界面。

```shell 终端
dotnet new masafx-service-cqrs --name TodoApp --no-https true --no-example true -db sqlite
cd TodoApp/src
dotnet new masablazor-empty-server --name TodoApp.Web --no-https true
dotnet add ./TodoApp.Web/TodoApp.Web.csproj reference ./TodoApp.Contracts/TodoApp.Contracts.csproj
dotnet add ./TodoApp.Web/TodoApp.Web.csproj package Masa.Contrib.Service.Caller.HttpClient -v 1.0.0
cd ../
dotnet sln add src/TodoApp.Web/TodoApp.Web.csproj
```

![项目结构图](https://cdn.masastack.com/framework/getting-started/web-project/1525201-20230419151038284-19118664.png)

## 定义实体

1. 我们在 `TodoApp.Service` 项目下的 **DataAccess/Entities** 文件夹中定义一个 `TodoEntity` 实体类。

   ```csharp DataAccess/Entities/TodoEntity.cs
   namespace TodoApp.Service.DataAccess;
   
   public class TodoEntity : Entity<Guid>
   {
       public string Title { get; set; }
   
       public bool Done { get; set; }
   }
   ```

2. 然后修改 `ExampleDbContext`，将 **TodoEntity** 添加进去，修改完后的 `ExampleDbContext.cs` 如下：

   ```csharp DataAccess/ExampleDbContext.cs l:5,19,20
   namespace TodoApp.Service.DataAccess;
   
   public class ExampleDbContext : MasaDbContext
   {
       public DbSet<TodoEntity> Todos { get; set; }
   
       public ExampleDbContext(MasaDbContextOptions<ExampleDbContext> options) : base(options)
       {
       }
   
       protected override void OnModelCreatingExecuting(ModelBuilder modelBuilder)
       {
           base.OnModelCreatingExecuting(modelBuilder);
           ConfigEntities(modelBuilder);
       }
   
       private static void ConfigEntities(ModelBuilder modelBuilder)
       {
           var todoBuilder = modelBuilder.Entity<TodoEntity>();
           todoBuilder.Property(e => e.Title).HasMaxLength(128);
       }
   }
   ```
   
3. 实体创建完成之后的目录结构如下所示：
   ![实体目录结构](https://cdn.masastack.com/framework/getting-started/web-project/create_entity_effect.png)

## 创建 Todo 后端接口服务

我们的 TodoApp 整个业务大概需要有以下接口：

   * Create：创建一个待办事项
   * Update：修改一个待办事项
   * Delete：删除一个待办事项
   * GetList：获取待办事项列表
   * Done：完成一个待办事项

### 创建 Dto

我们在 `TodoApp.Contracts` 项目中创建我们交换数据所需要的 Dto。它将被 Service 后端项目和 Blazor 前端项目共享，避免多次定义

   :::: code-group
   ::: code-group-item TodoGetListDto.cs
   ```csharp
   namespace TodoApp.Contracts;
   
   public class TodoGetListDto
   {
       public Guid Id { get; set; }
       
       public string Title { get; set; }
       
       public bool Done { get; set; }
   }
   ```
   :::
   ::: code-group-item TodoCreateUpdateDto.cs
   ```csharp
   namespace TodoApp.Contracts;
   
   public class TodoCreateUpdateDto
   {
       public string Title { get; set; }
   }
   ```
   :::
   ::::

创建完成之后的目录结构如下所示：

   ![Dto实体结构](https://cdn.masastack.com/framework/getting-started/web-project/create_dto.png)

### 使用 CQRS 模式

> 在这里我们使用 CQRS 来完成我们程序业务逻辑，在 CQRS 模式中我们的查询和其它业务操作是分开的。不了解 CQRS 的可以看看这篇文章：https://learn.microsoft.com/zh-cn/azure/architecture/patterns/cqrs

1. 我们先在 `TodoApp.Service` 项目下的 **Application\Commands** 目录中创建我们的业务命令：`CreateTodoCommand.cs`、`UpdateTodoCommand.cs`、`DeleteTodoCommand.cs`、`DoneTodoCommand.cs` 

   :::: code-group
   ::: code-group-item CreateTodoCommand.cs
   ```csharp
   namespace TodoApp.Service.Application.Commands;
   
   public record CreateTodoCommand(TodoCreateUpdateDto Dto) : Command { }
   ```
   :::
   ::: code-group-item UpdateTodoCommand.cs

   ```csharp
   namespace TodoApp.Service.Application.Commands;
   
   public record UpdateTodoCommand(Guid Id, TodoCreateUpdateDto Dto) : Command { }
   ```
   :::
   ::: code-group-item DeleteTodoCommand.cs

   ```csharp
   namespace TodoApp.Service.Application.Commands;
   
   public record DeleteTodoCommand(Guid Id) : Command { }
   ```
   :::
   ::: code-group-item DoneTodoCommand.cs
   ```csharp
   namespace TodoApp.Service.Application.Commands; 
   
   public record DoneTodoCommand(Guid Id, bool Done) : Command { }
   ```
   :::
   ::::

2. 在 `TodoApp.Service` 项目下的 **Application\Queries** 目录中创建我们的查询命令：`TodoGetListQuery.cs`

   ```csharp Application/Queries/TodoGetListQuery.cs
   namespace TodoApp.Service.Application.Queries;
   
   public record TodoGetListQuery : Query<List<TodoGetListDto>>
   {
       public override List<TodoGetListDto> Result { get; set; }
   }
   ```

3. 然后在 **Application** 目录中创建 `TodoQueryHandler.cs` 和 `TodoCommandHandler.cs` 类来处理应用程序所发送的命令 

   :::: code-group
   ::: code-group-item TodoQueryHandler.cs
   ```csharp
   using TodoApp.Service.Application.Queries;
   
   namespace TodoApp.Service.Application;
   
   public class TodoQueryHandler
   {
       readonly ExampleDbContext _todoDbContext;
   
       public TodoQueryHandler(ExampleDbContext todoDbContext) => _todoDbContext = todoDbContext;
   
       [EventHandler]
       public async Task GetListAsync(TodoGetListQuery query)
       {
           var todoDbQuery = _todoDbContext.Set<TodoEntity>().AsNoTracking();
           query.Result = await todoDbQuery.Select(e => new TodoGetListDto
           {
               Id = e.Id,
               Done = e.Done,
               Title = e.Title,
           }).ToListAsync();
       }
   }
   ```
   :::
   ::: code-group-item TodoCommandHandler.cs
   
   ```csharp
   using TodoApp.Service.Application.Commands;
   
   namespace TodoApp.Service.Application;
   
   public class TodoCommandHandler
   {
       readonly ExampleDbContext _todoDbContext;
   
       public TodoCommandHandler(ExampleDbContext todoDbContext) => _todoDbContext = todoDbContext;
   
       [EventHandler]
       public async Task CreateAsync(CreateTodoCommand command)
       {
           await ValidateAsync(command.Dto.Title);
           var todo = command.Dto.Adapt<TodoEntity>();
           await _todoDbContext.Set<TodoEntity>().AddAsync(todo);
           await _todoDbContext.SaveChangesAsync();
       }
   
       [EventHandler]
       public async Task UpdateAsync(UpdateTodoCommand command)
       {
           await ValidateAsync(command.Dto.Title, command.Id);
           var todo = await _todoDbContext.Set<TodoEntity>().AsNoTracking().FirstOrDefaultAsync(t => t.Id == command.Id);
           if (todo == null)
           {
               throw new UserFriendlyException("待办不存在");
           }
           command.Dto.Adapt(todo);
           _todoDbContext.Set<TodoEntity>().Update(todo);
           await _todoDbContext.SaveChangesAsync();
       }
   
       private async Task ValidateAsync(string title, Guid? id = null)
       {
           var todoExists = await _todoDbContext.Set<TodoEntity>().AnyAsync(t => t.Title == title && t.Id != id);
           if (todoExists)
               throw new UserFriendlyException("
               已存在");
       }
   
       [EventHandler]
       public async Task DeleteAsync(DeleteTodoCommand command)
       {
           var todo = await _todoDbContext.Set<TodoEntity>().AsNoTracking().FirstOrDefaultAsync(t => t.Id == command.Id);
           if (todo == null)
           {
               return;
           }
           _todoDbContext.Set<TodoEntity>().Remove(todo);
           await _todoDbContext.SaveChangesAsync();
       }
   
       [EventHandler]
       public async Task DoneAsync(DoneTodoCommand command)
       {
           var todo = await _todoDbContext.Set<TodoEntity>().AsNoTracking().FirstOrDefaultAsync(t => t.Id == command.Id);
           if (todo == null)
           {
               return;
           }
           todo.Done = command.Done;
           _todoDbContext.Set<TodoEntity>().Update(todo);
           await _todoDbContext.SaveChangesAsync();
       }
   }
   ```
   :::
   ::::


4. 创建完成之后，我们的目录结构如下：

   ![CQRS代码结构图](https://cdn.masastack.com/framework/getting-started/web-project/create_cqrs.png)

### 创建 MinimalApi 接口

在 `TodoApp.Service` 类库下的 **Services** 目录中创建一个 **TodoService.cs** 接口服务

   ```csharp Services/TodoService.cs
   using TodoApp.Service.Application.Commands;
   using TodoApp.Service.Application.Queries;
   
   namespace TodoApp.Service.Services;
   
   public class TodoService : ServiceBase
   {
       private IEventBus _eventBus => GetRequiredService<IEventBus>();
   
       public async Task<List<TodoGetListDto>> GetListAsync()
       {
           var todoQuery = new TodoGetListQuery();
           await _eventBus.PublishAsync(todoQuery);
           return todoQuery.Result;
       }
   
       public async Task CreateAsync(TodoCreateUpdateDto dto)
       {
           var command = new CreateTodoCommand(dto);
           await _eventBus.PublishAsync(command);
       }
   
       public async Task UpdateAsync(Guid id, TodoCreateUpdateDto dto)
       {
           var command = new UpdateTodoCommand(id, dto);
           await _eventBus.PublishAsync(command);
       }
   
       public async Task DeleteAsync(Guid id)
       {
           var command = new DeleteTodoCommand(id);
           await _eventBus.PublishAsync(command);
       }
   
       public async Task DoneAsync([FromQuery] Guid id, [FromQuery] bool done)
       {
           var command = new DoneTodoCommand(id, done);
           await _eventBus.PublishAsync(command);
       }
   }
   ```

### 修改 Program.cs 文件
 
分别为`添加 MinimalAPI`、`添加 Swagger`、`创建数据库`

   ```csharp Program.cs l:4-9,11-20,28-38
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddEventBus()
       .AddMasaDbContext<TodoAppDbContext>(opt =>
       {
           opt.UseSqlite();
       }) 
       .AddMasaMinimalAPIs(option => option.MapHttpMethodsForUnmatched = new string[] { "Post" })
       .AddAutoInject();
   
   builder.Services.AddEndpointsApiExplorer()
       .AddSwaggerGen(options =>
       {
           options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TodoAppApp", Version = "v1", Contact = new Microsoft.OpenApi.Models.OpenApiContact { Name = "TodoAppApp", } });
           foreach (var item in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml"))
           {
               options.IncludeXmlComments(item, true);
           }
           options.DocInclusionPredicate((docName, action) => true);
       });
   
   var app = builder.Build();
   
   app.UseMasaExceptionHandler();
   
   app.MapMasaMinimalAPIs();
   
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger().UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoAppApp"));
   
       #region MigrationDb
       using var context = app.Services.CreateScope().ServiceProvider.GetService<TodoAppDbContext>();
       {
           context!.Database.EnsureCreated();
       }
       #endregion
   }
   
   app.Run();
   
   ```

最终我们的接口服务就完成了，我们来启动下后端接口服务，并访问/swagger

   ![后端服务运行图](https://cdn.masastack.com/framework/getting-started/web-project/webapi_run_effect.png)



## 创建 Todo 前端界面

接下来我们将要开始编写我们的前端 web 界面了，在这之前我们再看下我们的 web 前端最终效果图

   ![界面UI图](https://cdn.masastack.com/framework/getting-started/web-project/result.png)

### 创建接口服务调用

>  我们的 web 程序需要调用后端接口来获取数据，我们得先创建后端接口服务调用。

在 `TodoApp.Web` 项目下的 **ApiCallers** 目录中分别创建 `TodoServiceOptions.cs` 和 `TodoCaller.cs`，前者是 todo 后端服务的配置，后者是接口调用

   :::: code-group
   ::: code-group-item TodoServiceOptions.cs
   ```csharp
   namespace TodoApp.Web.ApiCallers;
   
   public class TodoServiceOptions
   {
       public string BaseAddress { get; set; }
   }
   ```
   :::
   ::: code-group-item TodoCaller.cs
   ```csharp
   using Masa.Contrib.Service.Caller.HttpClient;
   using TodoApp.Contracts;
   using Microsoft.Extensions.Options;
   
   namespace TodoApp.Web.ApiCallers;
   
   public class TodoCaller : HttpClientCallerBase
   {
       protected override string BaseAddress { get; set; }
   
       public TodoCaller(IOptions<TodoServiceOptions> options)
       {
           BaseAddress = options.Value.BaseAddress;
           Prefix = "/api/v1/todoes";
       }
   
       public async Task<List<TodoGetListDto>> GetListAsync()
       {
           var result = await Caller.GetAsync<List<TodoGetListDto>>($"list");
           return result ?? new();
       }
   
       public async Task CreateAsync(TodoCreateUpdateDto dto)
       {
           var result = await Caller.PostAsync($"", dto);
       }
   
       public async Task UpdateAsync(Guid id, TodoCreateUpdateDto dto)
       {
           var result = await Caller.PutAsync($"{id}", dto);
       }
   
       public async Task DeleteAsync(Guid id)
       {
           var result = await Caller.DeleteAsync($"{id}", null);
       }
   
       public async Task DoneAsync(Guid id, bool done)
       {
           var result = await Caller.PostAsync($"done?id={id}&done={done}", null);
       }
   }
   ```
   :::
   ::::

创建完成的目录结构如下：

   ![](https://cdn.masastack.com/framework/getting-started/web-project/api_caller.png)

### 添加接口调用服务

添加后端接口调用服务，修改 `TodoApp.Web` 项目中的 `Program.cs` 文件

   ```csharp Program.cs l:8,9
   using TodoApp.Web.ApiCallers;
   
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddRazorPages();
   builder.Services.AddServerSideBlazor();
   
   builder.Services.AddMasaBlazor();
   builder.Services.Configure<TodoServiceOptions>(builder.Configuration.GetSection("TodoService"))
      .AddAutoRegistrationCaller(typeof(Program).Assembly);
   
   var app = builder.Build();
   
   if (!app.Environment.IsDevelopment())
   {
       app.UseHsts();
   }
   
   app.UseHttpsRedirection();
   
   app.UseStaticFiles();
   
   app.UseRouting();
   
   app.MapBlazorHub();
   app.MapFallbackToPage("/_Host");
   
   app.Run();
   ```

在这里我们还需要修改 **appsetting.json** 配置文件添加我们后端接口地址：

   ```json appsetting.json l:9-11
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "TodoService": {
       "BaseAddress": "your interface service address" //example  http://localhost:6001
     }
   }
   ```

### 实现 UI 界面

>  接下来我们就开始编写我们的界面了

* 修改 `TodoApp.Web` 项目下 **Pages** 目录中的 `Index.razor` 文件

   ```razor Pages/Index.razor
   @page "/"
   @using TodoApp.Contracts;
   @using TodoApp.Web.ApiCallers;
   <MContainer Style="max-width: 500px">
   
       <MTextField @bind-Value="_newTodo"
                   Label="What are you working on?"
                   Solo
                   OnKeyDown="OnEnterKeyDown">
           <AppendContent>
               <FadeTransition>
                   <MIcon If="@(!string.IsNullOrEmpty(_newTodo))"
                          OnClick="()=>Create()">
                       add_circle
                   </MIcon>
               </FadeTransition>
           </AppendContent>
       </MTextField>
   
       <h2 class="text-h4 success--text pl-4">
           Tasks:&nbsp;
           <FadeTransition LeaveAbsolute>
               <KeyTransitionElement Tag="span" Value="@($"task-{_tasks.Count}")">
                   @_tasks.Count
               </KeyTransitionElement>
           </FadeTransition>
       </h2>
   
       <MDivider></MDivider>
   
       <MRow Class="my-1" Align=AlignTypes.Center>
   
           <strong class="mx-4 info--text text--darken-2">
               Remaining: @RemainingTasks
           </strong>
           <MDivider Vertical></MDivider>
           <strong class="mx-4 success--text text--darken-2">
               Completed: @CompletedTasks
           </strong>
           <MSpacer></MSpacer>
           <MProgressCircular Value=Progress Class="mr-2"></MProgressCircular>
       </MRow>
   
       <MDivider Class="mb-4"></MDivider>
   
       @if (_tasks.Count > 0)
       {
           <MCard>
               <SlideYTransition>
                   @for (var i = 0; i < _tasks.Count; i++)
                   {
                       var task = _tasks[i];
                       if (i != 0)
                       {
                           <MDivider></MDivider>
                       }
                       <MListItem>
                           @if (editorTodoId == task.Id)
                           {
                               <MTextField Color="purple darken-2" @bind-Value="_updateTodo"></MTextField>
                           }
                           else
                           {
                               <MListItemAction>
                                   <MCheckbox TValue="bool" Value="@task.Done"
                                  ValueChanged="@(v => Done(task.Id,v))"
                                  Color="@(task.Done ? "grey" : "primary")">
                                       <LabelContent>
                                           <div class="@(task.Done ? "grey--text" : "primary--text") ml-4">
                                               @task.Title
                                           </div>
                                       </LabelContent>
                                   </MCheckbox>
                               </MListItemAction>
                           }
   
                           <MSpacer></MSpacer>
                           <MButton Icon Show="@(task.Done==false&&editorTodoId!=task.Id)" OnClick="()=>{editorTodoId=task.Id;_updateTodo=task.Title;}">
                               <MIcon>mdi-pencil</MIcon>
                           </MButton>
   
                           <MButton Outlined Small Show="@(editorTodoId==task.Id)" OnClick="()=>Update(task)" Color="success" Class="mr-2">
                               ok
                           </MButton>
                           <MButton Outlined Small Show="@(editorTodoId==task.Id)" OnClick="()=>editorTodoId=null">
                               canel
                           </MButton>
   
                           <MButton Icon Show="@(editorTodoId!=task.Id)" OnClick="()=>Delete(task.Id)" Color="error">
                               <MIcon>mdi-delete</MIcon>
                           </MButton>
                           <ScrollXTransition>
                               <MIcon If="@task.Done" Color="success">
                                   mdi-check
                               </MIcon>
                           </ScrollXTransition>
                       </MListItem>
                   }
               </SlideYTransition>
           </MCard>
       }
   
   </MContainer>
   
   @code {
   
       [Inject]
       public TodoCaller TodoCaller { get; set; }
   
       string _newTodo = "";
       string _updateTodo = "";
   
       private List<TodoGetListDto> _tasks = new();
   
       int CompletedTasks => _tasks.Count(t => t.Done);
   
       float Progress => _tasks.Count <= 0 ? 0 : (CompletedTasks * 100f) / _tasks.Count;
   
       int RemainingTasks => _tasks.Count - CompletedTasks;
   
       Guid? editorTodoId;
   
       async Task OnEnterKeyDown(KeyboardEventArgs eventArgs)
       {
           if (eventArgs.Key == "Enter")
           {
               await Create();
           }
       }
       protected override async Task OnInitializedAsync()
       {
           await base.OnInitializedAsync();
           await LoadListDataAsync();
       }
   
       private async Task LoadListDataAsync()
       {
           var result = await TodoCaller.GetListAsync();
           _tasks = result;
       }
   
       async Task Create()
       {
           await TodoCaller.CreateAsync(new TodoCreateUpdateDto { Title = _newTodo });
   
           await LoadListDataAsync();
           _newTodo = "";
       }
   
       async Task Done(Guid id, bool done)
       {
           await TodoCaller.DoneAsync(id, done);
           await LoadListDataAsync();
       }
   
       async Task Update(TodoGetListDto task)
       {
           await TodoCaller.UpdateAsync(task.Id, new TodoCreateUpdateDto { Title = _updateTodo });
           await LoadListDataAsync();
           editorTodoId = null;
       }
   
       async Task Delete(Guid id)
       {
           await TodoCaller.DeleteAsync(id);
           await LoadListDataAsync();
           editorTodoId = null;
       }
   }
   ```

## 运行程序

最后把我们的程序运行起来。先修改下 vs 的配置，配置启动多个项目

   ![](https://cdn.masastack.com/framework/getting-started/web-project/setting_project_start.png)

最终我们的程序如下图所示：

   ![](https://cdn.masastack.com/framework/getting-started/web-project/run_result.png)
