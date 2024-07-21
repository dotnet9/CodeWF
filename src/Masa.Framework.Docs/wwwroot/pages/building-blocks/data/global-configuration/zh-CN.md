## 全局配置

提供了以下属性或方法

* RootServiceProvider：`RootServiceProvider`，仅被初始化一次
* Build：用于构建 `RootServiceProvider`
* GetService：获取指定 `Service` 的实例，如果实例不存在，则返回`null`
* GetRequiredService：获取指定 `Service` 的实例（其中示例若不存在，则抛出未注册服务的异常）
* TrySetServiceCollection：尝试存储全局服务集合，当服务已存在则跳过
* SetServiceCollection：重新存储全局服务集合
* GetServices：获取全局服务集合
* TrySetAssemblies：尝试设置全局 `Assembly` 集合，如果已经设置则跳过
* SetAssemblies：设置全局 `Assembly` 集合
* GetAssemblies：获取全局 `Assembly` 集合，如果未设置全局 `Assembly` 集合则返回 `AppDomain.CurrentDomain.GetAssemblies()`
* TrySetJsonSerializerOptions：尝试设置全局`JsonSerializerOptions`配置，如果已设置则跳过
* SetJsonSerializerOptions：设置全局 `JsonSerializerOptions` 配置
* GetJsonSerializerOptions：返回全局 `JsonSerializerOptions` 配置，如果未设置则返回 `null`

全局配置用于提供默认值，例如：

注册 `MinimalAPIs` 时未指定需要扫描的服务所属的程序集集合，则默认使用全局 `Assembly` 集合，但如果注册 `MinimalAPIs` 时指定程序集集合，则以手动指定的为准, 其优先级为：

手动指定 > 全局配置

```csharp
builder.AddServices();
```

> 除了 `MinimalAPIs` 之外, 其余模块也满足 `手动指定` > `全局配置`的优先级规则