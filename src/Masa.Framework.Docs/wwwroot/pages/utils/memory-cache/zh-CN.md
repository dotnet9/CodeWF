# 内存缓存

## 概述

`Masa.Utils.Caching.Memory` 与 `Microsoft.Extensions.Caching.Memory` 没有任何关系，`Masa.Utils.Caching.Memory` 提供的缓存的生命周期取决于使用者，它提供了线程安全的字典集合，在使用方式上与 [`ConcurrentDictionary`](https://learn.microsoft.com/zh-cn/dotnet/api/system.collections.concurrent.concurrentdictionary-2) 类似，但它接受委托的方法也是支持线程安全的

## 使用

根据需要我们可以将 `MemoryCache<TKey, TValue>`构建为全局静态属性，那样一来它的生命周期是到应用结束，或者它被定义为某个类中的属性或者某个方法的普通变量，那么它的声明周期将与类或方法一致

1. 安装 `Masa.Utils.Caching.Memory`

   ```shell
   dotnet add package Masa.Utils.Caching.Memory
   ```

2. 使用缓存

   ```csharp l:5,10
   class Test
   {
       static void Main()
       {
           var cache = new MemoryCache<Guid, DateTime>();
           Guid id = Guid.NewGuid();
           cache.TryAdd(id, () => DateTime.Now);
   
           Guid newId = Guid.Parse(Console.ReadLine());
           if(cache.Get(newId, out DateTime? createTime))
           {
                Console.WriteLine("write time is: {Time}", createTime.Value);
           }
       }
   }
   ```