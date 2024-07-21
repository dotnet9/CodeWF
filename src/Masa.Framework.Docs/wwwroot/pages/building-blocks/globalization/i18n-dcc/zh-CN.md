# 国际化 - 分布式配置能力（DCC）

## 概述

`I18n` 让程序具备了支持多种语言能力，但它必须配置在本地配置文件中，具有一定的局限性，对于后期管理资源文件不太友好，MASA Framework 提供了 [`分布式配置中心-DCC`](/stack/dcc/introduce)的实现，通过它我们可以将多语言的资源内容在 `DCC` 上进行配置，后期管理资源文件可以在 `DCC` 的界面上进行操作，并且它是支持热更新的

## 使用

1. 以 `ASP.NET Core` 项目为例，安装 `Masa.Contrib.Globalization.I18n.AspNetCore`、 `Masa.Contrib.Globalization.I18n.Dcc`

   ```shell 终端
   dotnet add package Masa.Contrib.Globalization.I18n.AspNetCore
   dotnet add package Masa.Contrib.Globalization.I18n.Dcc
   ```

   > Masa.Contrib.Globalization.I18n.AspNetCore：通过中间件提供解析设置当前线程区域性的能力
   >
   > Masa.Contrib.Globalization.I18n.Dcc：为 I18n 提供远程配置的能力

2. 在 [Dcc](/stack/dcc/introduce) 配置多语言配置信息

   * en-US.json

     ![image-20230425200421281](https://cdn.masastack.com/framework/202304252004352.png)

   * zh-CN.json

     ![image-20230425200558437](https://cdn.masastack.com/framework/202304252005484.png)

3. 安装 [MasaConfiguration 并使用 DCC ](/framework/building-blocks/configuration/dcc)

   ```shell 终端
   dotnet add package Masa.Contrib.Configuration
   dotnet add package Masa.Contrib.Configuration.ConfigurationApi.Dcc
   ```

4. 配置 DCC 所需信息

   ```json appsettings.json
   {
     "DccOptions": {
       "ManageServiceAddress": "{Replace-Your-DccServiceAddress}",
       "AppId": "{Replace-Your-AppId}",
       "Environment": "{Replace-Your-Environment}",
       "Cluster": "{Replace-Your-Cluster}",
       "RedisOptions": 
       {
         "Servers":[
           {
             "Host": "localhost",
             "Port": 6379
           }
         ],
         "DefaultDatabase": 0,
         "Password": ""
       }
     }
   }
   
   ```

   > Redis 地址：记录 DCC 配置信息的 Redis 缓存服务地址

5. 配置支持语言

   ```json Resources/I18n/supportedCultures.json
   [
     {
       "Culture":"zh-CN",
       "DisplayName":"中文简体"
     },
     {
       "Culture":"en-US",
       "DisplayName":"English (United States)"
     }
   ]
   ```

6. 注册 `MasaConfiguration`、`I18n` 并使用 `I18n`

   ```csharp Program.cs l:3-10,14
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddMasaConfiguration(options =>
   {
       options.UseDcc();
   });
   builder.Services.AddI18n(null, options =>
   {
       options.UseDcc();
   });
   
   var app = builder.Build();
   
   app.UseI18n();
   ```

7. 使用 I18n

   ```csharp Program.cs l:3-4
   app.MapGet("/parameter/verify", (int page) =>
   {
       MasaArgumentException.ThrowIfLessThan(page, 1);
       return I18n.T("Tip.VerificationSucceeded");
   });
   ```

通过以上配置，我们将使用与`DCC`配置中默认 `AppId`，并读取名称为 `Culture.{语言}` 的配置对象，以上述例子来讲，由于我们支持的语言为 `zh-CN`、 `en-US`，因此我们将默认使用与 `DCC` 配置一致的默认 `AppId` 下的 `Culture.zh-CN` 、 `Culture.en-Us` 两个配置对象的值，后续如果需要管理资源的话，对应修改它们的值即可，无需重启应用，因为它们是支持热更新的 [如何管理对象](/stack/dcc/use-guide#section-914d7f6e5bf98c617ba17406)