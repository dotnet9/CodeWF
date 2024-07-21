# 国际化 - ASP.NET Core

## 概述

[`Masa.Contrib.Globalization.I18n.AspNetCore`](https://www.nuget.org/packages/Masa.Contrib.Globalization.I18n.AspNetCore)为 [I18n](/framework/building-blocks/globalization/i18n) 协助解析设置当前线程的区域性，对于 `ASP.NET Core Web` 项目来说，只需要使用它即可，它属于微软提供[本地化的中间件](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/localization#localization-middleware)的能力，它默认支持以下三种方式进行语言切换。

* URL 参数 方式：?culture=en-US，此方式优先级最高，格式为：culture=区域码

* Cookies 方式：cookie 格式：c=%LANGCODE%|uic=%LANGCODE%，其中 `c` 是 `Culture`，`uic` 是 `UICulture`，例如：

   > `UICulture` 控制读取哪个资源文件、`Culture` 控制格式化时使用的区域性格式 （不同国家的时间格式化标准不同）

   ``` cookie
   c=en-UK|uic=en-US
   ```

* 客户端浏览器语言自动匹配：如果前面两种方式都没有设置，支持自动根据客户端浏览器语言进行匹配

   语言优先级: URL 参数 方式 > Cookies方式 > 客户端语言 > 默认语言

   > 如果当前请求的语言不支持，则使用默认语言

## 使用

与 [`Masa.Contrib.Globalization.I18n`](/framework/building-blocks/globalization/i18n) 相比，它仅仅是增加了使用 `I18n` 中间件的操作，目的是解析设置当前线程的区域，完整代码如下

1. 安装`Masa.Contrib.Globalization.I18n.AspNetCore`

   ```shell 终端
   dotnet add package Masa.Contrib.Globalization.I18n.AspNetCore
   ```

2. 添加多语言资源文件

   :::: code-group
   ::: code-group-item en-US.json
   ```json Resources/I18n/en-US.json
   {
       "Home":"Home"
   }
   ```
   :::
   ::: code-group-item zh-CN.json
   ```json Resources/I18n/zh-CN.json
   {
       "Home":"首页"
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

3. 注册 `I18n`，并修改 `Program` 

   ```csharp
   builder.Services.AddI18n();
   ```

4. 使用 `I18N`

   ```csharp
   app.UseI18n();//Enable the middleware, finish parsing the request and set the region for the current request
   ```

5. 使用 `I18n`

   ```csharp
   app.Map("/test", (string key) => I18n.T(key));
   ```

## 高阶用法

### 默认语言

默认语言有两种配置方式，它们分别是:

* 手动指定默认语言
    * 通过`app.UseI18n("{Replace-Your-DefaultCulture}")`
* 约定配置
    * `supportedCultures.json`文件中的第一个语言

它们的优先级是：

手动指定默认语言 > 约定配置

## 其它

如果你的项目不属于 `ASP.NET Core Web` 项目，那么只需要安装 [`Masa.Contrib.Globalization.I18n`](https://www.nuget.org/packages/Masa.Contrib.Globalization.I18n)，除此之外，你需要配合 `UI` 完成对当前语言的设置即可，点击查看如何使用 [`Masa.Contrib.Globalization.I18n`](/framework/building-blocks/globalization/i18n)