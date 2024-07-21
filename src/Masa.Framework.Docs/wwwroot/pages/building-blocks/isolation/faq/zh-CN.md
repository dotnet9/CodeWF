# 隔离性 - 常见问题

## 公共

1. 如何修改隔离性默认读取的节点名 Isolation

   可通过以下两种方案来修改默认读取的节点名，两种方案任选其一即可

   :::: code-group
   ::: code-group-item 方案1

   ```csharp Program.cs l:2-8,12
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiTenant();
   }, options =>
   {
       options.SectionName = "CustomSectionName";
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.Run();
   ```
   :::
   ::: code-group-item 方案2
   ```csharp Program.cs l:2-11,15
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.Configure<IsolationOptions>(options =>
   {
       options.SectionName = "CustomSectionName";
   });
   
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiTenant();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.Run();
   ```
   :::
   ::::

2. 能否支持其它配置源而并非本地配置文件？

   支持，可通过以下两种方案支持其它配置源，两种方案任选其一即可

   * 方案1: 选项模式

   使用选项模式配置指定 `name` 的配置信息，以数据上下文为例

   ```csharp Program.cs l:3-29,33
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.Configure<IsolationOptions<ConnectionStrings>>(options =>
   {
       options.Data = new List<IsolationConfigurationOptions<ConnectionStrings>>()
       {
           new()
           {
               TenantId = "Tenant 1",
               Data = new ConnectionStrings(new List<KeyValuePair<string, string>>()
               {
                   new(ConnectionStrings.DEFAULT_CONNECTION_STRING_NAME, "{Replace-Your-Tenant1-ConnectionString}")
               })
           },
           new()
           {
               TenantId = "Tenant 2",
               Data = new ConnectionStrings(new List<KeyValuePair<string, string>>()
               {
                   new(ConnectionStrings.DEFAULT_CONNECTION_STRING_NAME, "{Replace-Your-Tenant2-ConnectionString}")
               })
           },
       };
   });
   
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiTenant();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.Run();
   ```

   > 数据上下文比较特殊，其 **name** 的值为**空**，不存在 **name** 不为空的情况，其余支持自定义 **name** 的模块，则需要通过 **builder.Services.Configure<IsolationOptions<TComponentConfig>>("自定义name的值"，options =>{ });** 设置即可 ( TComponentConfig 为组件的配置对象，详细可查看具体构建块的文档)

   * 方案2: 自定义 **IIsolationConfigProvider**

   通过自定义 **IIsolationConfigProvider** 的实现类来支持其它配置源

   :::: code-group
   ::: code-group-item 自定义隔离性配置提供程序
   ```csharp l:1-30
   public class CustomIsolationConfigProvider : IIsolationConfigProvider
   {
       /// <summary>
       /// Get the configuration information under the specified configuration node under the current tenant/environment (when null is returned, the default configuration information of the current component will be used)
       /// </summary>
       /// <param name="sectionName">Configuration node name (the configuration node names of different components are inconsistent (the configuration node name supports customization))</param>
       /// <param name="name">Name (the default is an empty string, and the building blocks that support factories support custom names)</param>
       /// <typeparam name="TComponentConfig">Component's configuration object</typeparam>
       /// <returns></returns>
       /// <exception cref="NotImplementedException"></exception>
       public TComponentConfig? GetComponentConfig<TComponentConfig>(string sectionName, string name = "") where TComponentConfig : class
       {
           // todo: Get the component configuration information in the current tenant/environment and return
           return default;
       }

       /// <summary>
       /// Get all configuration information of the specified configuration node
       /// </summary>
       /// <param name="sectionName">Configuration node name (the configuration node names of different components are inconsistent (the configuration node name supports customization))</param>
       /// <param name="name">Name (the default is an empty string, and the building blocks that support factories support custom names)</param>
       /// <typeparam name="TComponentConfig">component configuration object</typeparam>
       /// <returns></returns>
       public List<TComponentConfig> GetComponentConfigs<TComponentConfig>(string sectionName, string name = "") where TComponentConfig : class
       {
           //todo: Get all configuration information of the specified configuration node
           var list = new List<TComponentConfig>();
           return list;
       }
   }
   ```
   :::
   ::: code-group-item 使用自定义隔离性配置提供程序
   ```csharp Program.cs l:3-7,11
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddScoped<IIsolationConfigProvider, CustomIsolationConfigProvider>();
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiTenant();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.Run();
   ```
   :::
   ::::

