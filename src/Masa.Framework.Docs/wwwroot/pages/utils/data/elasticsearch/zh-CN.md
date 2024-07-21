# 数据 - Elasticsearch

## 概述

基于 [`NEST`](https://www.nuget.org/packages/NEST) 的扩展，其中封装了 `ES` 的常用操作，其中包括对 **索引**、**别名**、**文档**的管理

## 使用

1. 安装 `Masa.Utils.Data.Elasticsearch` 包

   ```shell 终端
   dotnet add package Masa.Utils.Data.Elasticsearch
   ```

2. 注册 `Elasticsearch`

   ```csharp Program.cs
   builder.Services.AddElasticsearch("http://localhost:9200"); // 或者builder.Services.AddElasticsearchClient("http://localhost:9200");
   ```

3. 使用 `ElasticClient`

   ```csharp
   IMasaElasticClient masaElasticClient;//从 DI 获取 `IMasaElasticClient`
   masaElasticClient.CreateIndexAsync("{Replace-Your-IndexName}");
   ```

## 源码解读

通过注册 `Elasticsearch`，我们可以从 `DI` 中获取 `IMasaElasticClient` 与 `IElasticClient` ，其中 `IMasaElasticClient` ，而 `IElasticClient` 是 [Nest](https://www.nuget.org/packages/NEST) 对 `ES` 的抽象

### IMasaElasticClient

对`ES`常用操作的抽象，包括对 **索引**、**别名**、**文档** 的管理

#### 索引

* IndexExistAsync：判断索引是否存在
* CreateIndexAsync：创建索引
* DeleteIndexAsync：删除指定索引
* DeleteMultiIndexAsync：删除指定的索引集合
* DeleteIndexByAliasAsync：根据别名删除索引
* GetAllIndexAsync：得到当前 `ES` 服务所有的索引
* GetIndexByAliasAsync：得到指定别名的索引

#### 别名

* GetAllAliasAsync：得到当前 `ES` 服务所有的别名
* GetAliasByIndexAsync：根据索引名得到别名信息
* BindAliasAsync：为指定索引绑定别名
* UnBindAliasAsync：为指定索引解除绑定别名

### 文档

* DocumentExistsAsync：判断文档是否存在
* DocumentCountAsync:　得到指定索引下所有的文档总数
* CreateDocumentAsync：新增单个文档
* CreateMultiDocumentAsync：新增多个文档
* SetDocumentAsync：设置文档（已存在文档会覆盖，等同于 `Upsert`）
* DeleteDocumentAsync：根据文档 `ID` 删除指定文档
* DeleteMultiDocumentAsync：根据文档id集合删除文档
* ClearDocumentAsync：根据索引或者别名清空文档
* UpdateDocumentAsync：更新单个文档
* UpdateMultiDocumentAsync：更新文档集合
* GetAsync：根据文档 `ID` 得到文档详情
* GetMultiAsync：根据文档 `ID` 集合得到文档集合
* GetListAsync：得到文档列表
* GetPaginatedListAsync：得到文档分页列表

### IElasticClient

`IElasticClient` 是 [`NEST`](https://github.com/elastic/elasticsearch-net) 为 `ES` 提供功能的抽象，详细用法可[查看](https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/elasticsearch-net-getting-started.html)

### IMasaElasticClientFactory

`IMasaElasticClient` 的工厂抽象，可用于创建指定 `name` 的 [`IMasaElasticClient`](#IMasaElasticClient)

### IElasticClientFactory

`IElasticClient` 的工厂抽象，可用于创建指定 `name` 的 [`IElasticClient`](#IElasticClient)

## 常见问题

* 出错提示为：`"Content-Type header [application/vnd.elasticsearch+json; compatible-with=7] is not supported"`

  我们默认启用兼容模式，即`EnableApiVersioningHeader(true)`，这样对8.*版本支持很好，但在部分7.*会导致错误，此时需要手动关闭兼容模式，即`EnableApiVersioningHeader(false)`。

    ```csharp
    service.AddElasticsearchClient("es", option =>
    {
        option.UseNodes("http://localhost:9200")
            .UseConnectionSettings(setting => setting.EnableApiVersioningHeader(false));
    });
    ```

[为何开启兼容模式？](https://github.com/elastic/elasticsearch-net/issues/6154)

## 参考文献

* [索引别名](https://www.elastic.co/guide/cn/elasticsearch/guide/current/index-aliases.html)