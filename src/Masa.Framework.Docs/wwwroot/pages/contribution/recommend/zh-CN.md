# 编码风格与代码规范

好的编码风格不仅仅使得我们的项目的可读性更强, 同时它也使得项目更加的更优雅, 为此我们建议大家按照以下推荐进行编码

## 使用统一版本的包

对同一系列的包使用同一版本有助于避免因为版本不一致而出现各种各样的奇怪 bug, 我们建议增加全局配置, 通过全局配置来解决此问题

1. 在解决方案根目录增加配置文件，使用特定版本的 nuget 包, 这里我们以`1.0.0-preview.1`版本为例

```xml Directory.Build.props
<Project>
  <PropertyGroup>
    <MasaFrameworkPackageVersion>1.0.0-preview.1</MasaFrameworkPackageVersion>
  </PropertyGroup>
</Project>
```

> 如果遇到`IDE`**不能正确识别包版本号**的情况, 请再次检查文件名, 确保其**扩展名**为**props**, 而不是扩展名为.txt的文件

后续升级版本时修改`MasaFrameworkPackageVersion`的值为对应版本即可

2. 修改项目配置文件，使用全局配置文件中指定包的版本，以`Masa.Contrib.Service.MinimalAPIs`为例

```xml XXX.csproj
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Masa.Contrib.Service.MinimalAPIs" Version="$(MasaFrameworkPackageVersion)" />
  </ItemGroup>
</Project>
```

## 全局引用

使用全局引用代替局部引用, 避免每个类中都需要引入命名空间

在类库根目录新增名子为 `_Imports` 的类, 其中引入当前类库使用的命名空间, 例如:

```csharp Imports.cs
global using System.Linq.Expressions;
```

> 对全局引用命名空间排序（Visual Studio 中使用 **Ctrl+K+E**）

### 类命名

在文档中会发现很多地方有推荐命名要求, 它们都不是强制性的, 但我们仍然建议大家能按照推荐进行命名

| 后缀               | 描述                                                                                                                                        |
|------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| Service          | API服务                                                                                                                                     |
| DomainEvent      | 领域事件                                                                                                                                      |
| DomainService    | 领域服务                                                                                                                                      |
| Repository       | 仓储                                                                                                                                        |
| DbContext        | 数据上下文                                                                                                                                     |
| Event            | 进程内事件                                                                                                                                     |
| EventHandler     | 事件处理程序                                                                                                                                    |
| DomainEventHandler | 领域事件处理程序                                                                                                                                  |
| Command          | 写命令的进程内事件                                                                                                                                 |
| Query            | 读命令的进程内事件                                                                                                                                 |
| DomainCommand    | 写命令的领域事件                                                                                                                                  |
| DomainQuery      | 读命令的领域事件                                                                                                                                  |
| IntegrationDomainEvent | 集成领域事件 (服务的发布与订阅不在同一个进程中)                                                                                                                 |
| EntityTypeConfiguration | 数据库表与实体映射关系, [相关文档](https://learn.microsoft.com/zh-cn/dotnet/api/system.data.entity.modelconfiguration.entitytypeconfiguration-1)（EFCore） |

## 命名规范

### 基本要求

* 避免无意义的方法、参数命名，使用与职责一致的命名，做到 **看词识意** (如果不能做到, 则需提供 **注释** 用来辅助理解)
* 不随意使用 **Public** 访问级别 (对外提供必要的方法或者成员，针对不需要外部访问的，请使用更严格的访问级别，避免造成更高的学习成本)

### 枚举命名规范

非位标志的枚举类型使用单数, 例如：

```csharp Season.cs
enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}
```

位标志的枚举类型使用复数，例如：

```csharp Days.cs
[Flags]
public enum Days
{
    None      = 0b_0000_0000,  // 0
    Monday    = 0b_0000_0001,  // 1
    Tuesday   = 0b_0000_0010,  // 2
    Wednesday = 0b_0000_0100,  // 4
    Thursday  = 0b_0000_1000,  // 8
    Friday    = 0b_0001_0000,  // 16
    Saturday  = 0b_0010_0000,  // 32
    Sunday    = 0b_0100_0000,  // 64
    Weekend   = Saturday | Sunday
}
```