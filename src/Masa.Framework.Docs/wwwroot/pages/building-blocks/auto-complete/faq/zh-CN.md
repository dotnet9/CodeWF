# 自动补全 - 常见问题

## 概述

记录了使用 **自动补全** 可能遇到的问题以及问题应该如何解决

## ElasticSearch

1. 出错提示为：`"Content-Type header [application/vnd.elasticsearch+json; compatible-with=7] is not supported"`

   我们默认启用兼容模式，即`EnableApiVersioningHeader(true)`，这样对8.*版本支持很好，但在部分7.*会导致错误，此时需要手动关闭兼容模式，即`EnableApiVersioningHeader(false)`。

   ```csharp
   service.AddElasticsearchClient("es", option =>
   {
       option.UseNodes("http://localhost:9200")
           .UseConnectionSettings(setting => setting.EnableApiVersioningHeader(false));
   });
   ```

   [为何开启兼容模式？](https://github.com/elastic/elasticsearch-net/issues/6154)