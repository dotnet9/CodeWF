# 服务调用 -常见问题

记录了使用 **服务调用** 可能遇到的问题以及问题应该如何解决

## 公共

1. 自动注册自定义 `Caller` 失败的情况？

   检查下继承 `HttpClientCallerBase` 的类与 `AddAutoRegistrationCaller`方法是否不在一个程序集，如果是，则可能会出现可通过下面提供的任一方案解决：

   * 指定Assembly集合 (仅对当前Caller有效)

      ```csharp
      var assemblies = typeof({Replace-With-Your-CustomCaller}).Assembly;
      builder.Services.AddAutoRegistrationCaller(assemblies);
      ```

   * 设置全局 Assembly 集合（影响全局 Assembly 默认配置，设置错误的 Assembly 集合会导致其它使用全局 Assembly 的服务出现错误）
   
      ```csharp
      var assemblies = typeof({Replace-With-Your-CustomCaller}).Assembly;
      MasaApp.SetAssemblies(assemblies);
      builder.Services.AddAutoRegistrationCaller();
      ```

## HttpClient

1. 继承 `HttpClientCallerBase` 的实现类如何获取服务？

   可通过构造函数注入所需服务

2. 继承 `HttpClientCallerBase` 的实现类的生命周期是什么？

   默认声明周期为: `Scoped`，可在使用 `AddAutoRegistrationCaller`  时指定生命周期为 `Singleton`、`Transient`

## DaprClient

1. 继承 `DaprCallerBase` 的实现类如何获取服务？

   可通过构造函数注入所需服务

2. 继承 `DaprCallerBase` 的实现类的生命周期是什么？

   默认声明周期为: `Scoped`，可在使用 `AddAutoRegistrationCaller`  时指定生命周期为 `Singleton`、`Transient`
