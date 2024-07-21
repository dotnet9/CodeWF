# 数据 - 数据过滤

在 `Masa.BuildingBlocks.Data.Contracts` 中提供了数据过滤的功能，我们定义了一些支持开箱即用的过滤

## 功能

### 软删除

实现 `ISoftDelete` 的实体支持软删除，当执行删除操作时，我们将把当前数据标记为删除，它并不是物理删除，数据仍然保存在数据库中，但默认查询时会过滤掉已经被标记删除的数据

```csharp
public class Student : ISoftDelete
{
    public int Id { get; set; }

    public string Name { get; set; }
    
    public bool IsDeleted { get; private set; }
}
```

### 多租户

实现 `IMultiTenant` 的实体支持多租户，通常情况下使用逻辑隔离时，使用它更为合适,
它的本质是在默认查询后增加租户 `ID` 等于`当前请求的租户 ID` 条件，以满足默认数据过滤的功能，如果租户是通过物理隔离（每个租户使用一个全新的数据库时，它的用处不是很大）

```csharp
public class User : IMultiTenant
{
    public int Id { get; set; }

    public string Name { get; set; }
    
    public Guid TenantId { get; set; }
}
```

> 如果你的租户 `ID` 不是 `Guid` 类型时，可通过修改继承接口为 `IMultiTenant<T>` 以支持其他类型的租户 `ID` ，例如: `IMultiTenant<int>`

### 多环境

实现 `IMultiEnvironment` 的实体支持多环境，通常情况下使用逻辑隔离时，使用它更为合适，它的本质是在默认查询后增加环境等于`当前请求所处环境`
条件，以满足默认数据过滤的功能 如果环境是通过物理隔离 (每个环境使用一个全新的数据库时，它的用处不是很大)

```csharp
public class User : IMultiEnvironment
{
    public int Id { get; set; }

    public string Name { get; set; }
    
    public string Environment { get; set; }
}
```

## 使用

我们提供了三种开箱即用的过滤，在查询时会自动过滤数据，如果我们希望临时关闭过滤的功能，比如提供一个回收站功能，可以通过 `IDataFilter` 提供临时关闭软删除功能以便查询出已经被标记删除的数据，例如:

```csharp
public class CatalogItemService: IScopedDependency
{
    private readonly CatalogDbContext _dbContext; 
    private readonly IDataFilter _dataFilter;

    public CatalogItemService(CatalogDbContext dbContext, IDataFilter dataFilter)
    {
        _dbContext = dbContext;
        _dataFilter = dataFilter;
    }

    public Task<List<CatalogItem>> GetCrecycleListAsync()
    {
        using (_dataFilter.Disable<ISoftDelete>())
        {
            return _dbContext.Set<CatalogItem>().Where(u => u.IsDeleted).ToListAsync();
        }
    }
}
```