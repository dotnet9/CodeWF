# 用户身份 - 常见问题

## 公共

1. A: 我们的项目 `Claim` 的 `value` 值不是通过 `Json` 序列化的，而是通过 `Yaml` 或者其它格式来序列化的，这样可能会导致我们的项目无法读取，请问如何解决？

   Q: 以 `Yaml` 为例：

   ```csharp Program.cs l:2-3
   var services = new ServiceCollection();
   services.AddSerialization(nameof(DataType.Yml), builder => builder.UseYaml());
   services.AddMasaIdentity(nameof(DataType.Yml));
   ```