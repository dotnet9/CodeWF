# 0.7.0 升级指南

## 重构

1. <font color=Red>全局异常</font>(/framework/building-blocks/exception)进行了<font color=Red>重构</font>，框架原生<font color=Red>支持</font>了[多语言](/framework/building-blocks/globalization/overview)，需要更换引用依赖包

    * `Masa.Utils.Exceptions` -> `Masa.Contrib.Exceptions` 
    * 全局异常

        :::: code-group
        ::: code-group-item 安装包
        ```shell 终端
        dotnet add package Masa.Contrib.Exceptions
        ```
        :::
        ::: code-group-item 使用全局异常 + I18n 
        ```csharp Program.cs
        var builder = WebApplication.CreateBuilder(args);
     
        var app = builder.Build();

        app.UseMasaExceptionHandler();
        ```
        :::
        ::: code-group-item 自定义处理异常
        ```csharp Program.cs
        var builder = WebApplication.CreateBuilder(args);
   
        var app = builder.Build();
   
        app.UseMasaExceptionHandler(opt =>
        {
            opt.ExceptionHandler = context =>
            {
                if (context.Exception is ValidationException validationException)
                {
                    context.ToResult(validationException.Errors.Select(err => err.ToString()).FirstOrDefault()!);
                }
            };
        });
        ```
        :::
        ::::

    * <font color=Red>全局异常 + I18n</font>
      1. 安装包 `Masa.Contrib.Globalization.I18n.AspNetCore`
      2. 默认资源

          :::: code-group
          ::: code-group-item 文件夹结构
          ```shell 
          - Resources
              - I18n
                  - en-US.json
                  - zh-CN.json
                  - supportedCultures.json
          ```
          :::
          ::: code-group-item en-US.json
          ```json Resources/I18n/en-US.json
          {
              "Home":"Home",
              "Exception":{
                  "NotFound":"Not Found {0}"
              }
          }
          ```
          :::
          ::: code-group-item zh-CN.json
          ```json Resources/I18n/zh-CN.json
          {
              "Home":"首页",
              "Exception":{
                  "NotFound":"没有找到{0}"
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
      
      3. 注册并使用 `I18n`
      
          ```csharp Program.cs
          var builder = WebApplication.CreateBuilder(args);
          builder.Services.AddI18n();

          var app = builder.Build(); 
          app.UseMasaExceptionHandler();
          app.UseI18n();
          ```

      4. 使用 `I18n`
         
         ```csharp Program.cs
         app.MapGet("exception", () =>
         {
             throw new UserFriendlyException(errorCode: "Exception.NotFound", "用户");
         });
         
         app.MapGet("home", () =>
         {
             return I18n.T("Home");
         });
         ```

2. `DaprStarter` 重构，优化与 [MasaConfiguration](/framework/building-blocks/configuration/overview)、[服务调用](/framework/building-blocks/caller/overview)使用繁琐的问题，需要更换引用依赖包

    * `Masa.Utils.Development.Dapr` -> `Masa.Contrib.Development.DaprStarter`
    * `Masa.Utils.Development.Dapr.AspNetCore` -> `Masa.Contrib.Development.DaprStarter.AspNetCore`

3. 数据规约重构，与 `EFCore` 解耦，需要更换引用依赖包

    * `Masa.Contrib.Data.Contracts.EFCore`重构并更名为`Masa.Contrib.Data.Contracts`

## 功能

1. 新增加[多语言](/framework/building-blocks/globalization/overview)支持
2. 新增加[规则引擎](/framework/building-blocks/rule-engine)支持
3. 增加[基于 FluentValidation 的事件中间件](/framework/building-blocks/dispatcher/local-event#section-4e8b4ef69a8c8bc14e2d95f44ef6)