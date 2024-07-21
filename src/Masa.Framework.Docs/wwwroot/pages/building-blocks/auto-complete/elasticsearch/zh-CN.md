# 自动补全 - Elasticsearch

## 概述

基于 [elasticsearch](https://www.elastic.co/cn/elasticsearch/) 实现的自动补全，相比关系型数据库，它拥有更高的查询性能，通过其插件，我们可以更轻松的实现分词搜索，它将帮助我们的自动补全更加智能

## 必要条件

* 确保有一个可用的 `elasticsearch` 服务，并安装了以下插件
  * [elasticsearch-analysis-ik](https://github.com/medcl/elasticsearch-analysis-ik)
  * [pinyin](https://github.com/medcl/elasticsearch-analysis-pinyin)

## 使用

1. 安装 `Masa.Contrib.SearchEngine.AutoComplete.ElasticSearch`

   ```shell 终端
   dotnet add package Masa.Contrib.SearchEngine.AutoComplete.ElasticSearch
   ```

2. 注册 `AutoComplete`

   ```csharp Program.cs
   var services = new ServiceCollection();
   services.AddAutoComplete<long>(optionsBuilder =>
       {
           optionsBuilder.UseElasticSearch(options =>
           {
               options.ElasticsearchOptions.UseNodes("http://localhost:9200");
               options.IndexName = "user_index_01";
               options.Alias = "user_index";
           });
       }
   );
   ```

3. 创建 `AutoCompleteClient` （初始化索引）

   ```csharp
   var autoCompleteClient = serviceProvider.GetRequiredService<IAutoCompleteClient>();
   await autoCompleteClient.BuildAsync();
   ```

   > 如果不创建直接写入数据，则会出现查询结果搜索不到的情况
   >
   > 如果当前 `AutoCompleteClient`已经成功创建，则多次调用时将会跳过

4. 设置数据

   ```csharp
   var autoCompleteDocument = new AutoCompleteDocument<long>()
   {
       Id = "{es-document-id}",
       Text = "{es-document-search content}",//Enter what to search
       Value = 0//Enter the searched value
   };
   await autoCompleteClient.SetAsync(autoCompleteDocument);
   ```

5. 获取数据

   ```csharp
   var document = await autoCompleteClient.GetAsync<long>("{es-document-search content}");
   ```

6. 删除数据

   ```csharp
   await autoCompleteClient.DeleteAsync("{es-document-id}");
   ```

## 高阶用法

### 使用自定义模型 

> 自动补全除了必须的 `Text`（显示内容）、`Value`（值）之外，还需要更多的信息，不想再多查询一次 `Caching （缓存）` 或 `Database （数据库）`

1. 新建 `UserAutoCompleteDocument`，并<font Color=RED>继承</font> `AutoCompleteDocument`

   ```csharp
   public class UserAutoCompleteDocument : AutoCompleteDocument
   {
       public int Id { get; set; }
   
       public string Name { get; set; }
   
       public string Phone { get; set; }
   
       protected override string GetText()
       {
           return $"{Name}:{Phone}";
       }
   
       /// <summary>
       /// Use user id as es document id
       /// </summary>
       /// <returns></returns>
       public override string GetDocumentId() => Id.ToString();
   }
   ```

2. 注册 `AutoComplete`

   ```csharp Program.cs
   var services = new ServiceCollection();
   services.AddAutoCompleteBySpecifyDocument<UserAutoCompleteDocument>(optionsBuilder =>
   {
       optionsBuilder.UseElasticSearch(options =>
       {
           options.ElasticsearchOptions.UseNodes("http://localhost:9200");
           options.IndexName = "user_index_01";
           options.Alias = "user_index";
       });
   });
   ```

3. 创建 `AutoCompleteClient`（初始化索引）

   ```csharp
   var autoCompleteClient = serviceProvider.GetRequiredService<IAutoCompleteClient>();
   await autoCompleteClient.BuildAsync();
   ```

4. 设置数据

   ```csharp
   var userAutoCompleteDocument = new UserAutoCompleteDocument()
   {
       Id = 1,
       Name = "MASA",
       Phone = "13999999999"
   };
   await autoCompleteClient.SetBySpecifyDocumentAsync(userAutoCompleteDocument);
   ```

5. 获取数据

   ```csharp
   var userAutoCompleteDocument = await autoCompleteClient.GetBySpecifyDocumentAsync<UserAutoCompleteDocument>("masa");
   ```

6. 删除数据

   ```csharp
   await autoCompleteClient.DeleteAsync("1");
   ```