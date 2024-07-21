# 规则引擎 - Microsoft

## 概述

基于 [`RulesEngine`](https://github.com/microsoft/RulesEngine) 实现的规则引擎，它提供了一种简单的方法，使您能够将规则放在系统核心逻辑之外的存储中，从而确保规则的任何更改都不会影响核心系统

## 使用

1. 安装 `Masa.Contrib.RulesEngine.MicrosoftRulesEngine`

   ```shell 终端
   dotnet add package Masa.Contrib.RulesEngine.MicrosoftRulesEngine
   ```

2. 注册规则引擎

   ```csharp Program.cs
   var services = new ServiceCollection();
   services.AddRulesEngine(rulesEngineOptions =>
   {
       rulesEngineOptions.UseMicrosoftRulesEngine();
   });
   ```

3. 使用规则引擎

   ```csharp
   var json = @"{
     ""WorkflowName"": ""UserInputWorkflow"",// optional
     ""Rules"": [
       {
         ""RuleName"": ""CheckAge"",
         ""ErrorMessage"": ""Must be over 18 years old."",
         ""ErrorType"": ""Error"",
         ""RuleExpressionType"": ""LambdaExpression"",
         ""Expression"": ""Age > 18""
       }
     ]
   }";//规则json
   
   var rulesEngineClient = serviceProvider.GetRequiredService<IRulesEngineClient>();
   var result = rulesEngineClient.Execute(ruleJson, new
   {
       Age = 19
   });
   Console.WriteLine("规则执行结果为{0}", result[0].IsValid);
   ```

## 高阶用法

### 扩展支持的方法

默认规则引擎不支持除 `System` 命名空间以外的方法，但可以通过更改默认配置支持其它方法

1. 新建 `StringUtils` 类，用于扩展字符串方法，并为规则引擎中提供扩展方法

   ```csharp
   public static class StringUtils
   {
       public static bool IsNullOrEmpty(this string? value)
           => string.IsNullOrEmpty(value);
   }
   ```

2. 注册规则引擎，并扩展支持 `StringUtils`类提供的方法

   ```csharp Program.cs
   builder.Services.AddRulesEngine(rulesEngineOptions =>
   {
       rulesEngineOptions.UseMicrosoftRulesEngine(new ReSettings()
       {
           CustomTypes = new[] { typeof(StringUtils) }
       });
   })
   ```