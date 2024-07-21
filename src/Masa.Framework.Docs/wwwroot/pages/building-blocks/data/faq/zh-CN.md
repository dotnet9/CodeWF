# 数据 -常见问题

记录了使用 **数据** 可能遇到的问题以及问题应该如何解决

## EFCore

1. 如何修改 `XXXDbContext` 所读取的节点名称?

   通过 `ConnectionStringName` 特性可自定义当前上下文读取的节点，例如：

   :::: code-group
   ::: code-group-item CustomDbContext

   ```csharp Infrastructure/CustomDbContext.cs
   [ConnectionStringName("Custom")]
   public class CustomDbContext : MasaDbContext<CustomDbContext>
   {
       public CustomDbContext(MasaDbContextOptions<CustomDbContext> options) : base(options)
       {
       }
   }
   ```

   :::
   ::: code-group-item appsettings.json

   ```json appsettings.json
   {
     "ConnectionStrings": {
       "Custom": "{Replace-Your-Read-DbConnectionString}"
     }
   }
   ```

   :::
   ::::

2. 如何修改 `XXXDbContext` 默认全局跟踪？

   :::: code-group
   ::: code-group-item 方案1

   ```csharp Infrastructure/CustomDbContext.cs
   public class CustomDbContext : MasaDbContext<CustomDbContext>
   {
       public CustomDbContext(MasaDbContextOptions<CustomDbContext> options) : base(options)
       {
           ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
       }
   }
   ```

   :::
   ::: code-group-item 方案2

   ```csharp Infrastructure/CustomDbContext.cs
   var services = new ServiceCollection();
   services.AddMasaDbContext<CustomDbContext>(masaDbContextBuilder =>
   {
       masaDbContextBuilder.UseInMemoryDatabase("{Replace-Your-Database-Name}");
       masaDbContextBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
   });
   ```

   :::
   ::::

3. `MasaDbContext` 不支持 `EFCore 7.0`、`EFCore 8.0`

   `MasaDbContext` 未限制 `EFCore` 的版本，但如果使用 `EFCore 6.0` 以上版本时，请确保将 `Microsoft.EntityFrameworkCore.XXX` 全系升级到相同版本，以 `SqlServer` 数据库为例：

   原本配置中仅安装 `Masa.Contrib.Data.EFCore.SqlServer` ，当安装了 7.0.5 版本的`Microsoft.EntityFrameworkCore.Tools`后，还需额外安装：

   ```shell 终端
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 7.0.5
   ```

   查看 **XXX.csproj** 文件：

   ```xml
   <Project Sdk="Microsoft.NET.Sdk.Web">
   
     <PropertyGroup>
       <TargetFramework>net7.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
     </PropertyGroup>
   
     <ItemGroup>
       <PackageReference Include="Masa.Contrib.Data.EFCore.SqlServer" Version="1.0.0" />
       <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.5">
         <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
         <PrivateAssets>all</PrivateAssets>
       </PackageReference>
       <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.5" />
     </ItemGroup>
   
     <ItemGroup>
       <ProjectReference Include="..\Masa.EShop.Contracts.Catalog\Masa.EShop.Contracts.Catalog.csproj" />
     </ItemGroup>
   
   </Project>
   
   ```

   > 若在 Masa.Contrib.Data.EFCore.XXX 中引用了 `Microsoft.EntityFrameworkCore.XXX`相关的包，则需要在业务项目中进行安装，并使用相同版本
