# 实战教程 - 第四章: 全局异常

## 概述

本章将使用[全局异常](/framework/building-blocks/exception)，通过它我们可以对外输出格式统一且更加友好的错误信息

## 开始

1. 选中 `Masa.EShop.Service.Catalog` 项目并安装 `Masa.Contrib.Exceptions`

   ```shell 终端
   dotnet add package Masa.Contrib.Exceptions -v 1.0.0
   ```

2. 使用[全局异常](/framework/building-blocks/exception)

   ```csharp Program.cs l:7
   var builder = WebApplication.CreateBuilder(args);
   
   -----Ignore the rest of the service registration-----
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   app.UseMasaExceptionHandler();
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   app.Run();
   ```

3. 通过 Swagger 测试，以创建商品为例

   <div>
     <img alt="Create Product" src="https://cdn.masastack.com/framework/tutorial/mf-part-4/swagger.png"/>
   </div>


4. 自定义异常处理

   例如：当出现 `ArgumentNullException` 异常时，对外输出具体错误信息，Http 状态码为：298

   ```csharp Program.cs
   app.UseMasaExceptionHandler(options =>
   {
       options.ExceptionHandler = exceptionContext =>
       {
           if (exceptionContext.Exception is ArgumentNullException ex)
               exceptionContext.ToResult(ex.Message, 298);
       };
   });
   ```

   > 查看[文档](/framework/building-blocks/exception#section-4e2d95f44ef6)了解更多自定义异常处理的写法

## 总结

通过全局异常功能，我们可以在项目任何地方中断操作，减少了很多繁琐的工作，除此之外通过定制异常并输出格式一致的响应信息，它将极大的方便与前端工程师协作开发，除此之外，全局异常支持与 [I18n](/framework/building-blocks/globalization/i18n) 协作，输出更友好的错误信息