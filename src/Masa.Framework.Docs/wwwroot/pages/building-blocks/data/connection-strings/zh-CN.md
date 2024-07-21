# 数据 - 连接字符串

## 概述

通过为上下文增加 **[ConnectionStringName]** 特性来修改当前数据上下文读取的节点

## 使用

1. 配置默认数据库地址以及读库地址

   ```json appsettings.json l:3
   {
     "ConnectionStrings": {
       "ReadDbConnection": "{Replace-Your-Read-DbConnectionString}"
     }
   }
   ```

2. 新建数据上下文

   ```csharp Infrastructure/OrderReadDbContext.cs l:1
   [ConnectionStringName("ReadDbConnection")]
   public class OrderReadDbContext: MasaDbContext<OrderReadDbContext>
   {
       public OrderReadDbContext(MasaDbContextOptions<OrderReadDbContext> options) : base(options)
       {
       }
   }
   ```
   
   > 设置读库上下文所属用的数据库链接字符串名称为 `ReadDbConnection`

## 其它 

当数据上下文<font color=Red> 未添加 [ConnectionStringName] 特性</font>或者<font color=Red>指定的 Name </font>为<font color=Red>空</font>或者<font color=Red> null </font>时，<font color=Red>默认读取节点为 DefaultConnection</font>，否则使用的节点名为指定的 `Name`

