namespace CodeWF.EntityFramework;

public static class Extension
{
    public static void AddCodeWFEntityFramework(this IServiceCollection services, Action<CMSEFCoreConfig> action)
    {
        var config = new CMSEFCoreConfig();
        action?.Invoke(config);

        services.AddKnownEntityFramework(option =>
        {
            // 配置数据库
            option.OnConfig = c => c.UseSqlServer(config.ConnString);
            // 在此配置业务库数据模型
            option.OnModel = m =>
            {
                m.Entity<CmCategory>().HasKey(x => x.Id);
                m.Entity<CmLog>().HasKey(x => x.Id);
                m.Entity<CmPost>().HasKey(x => x.Id);
                m.Entity<CmReply>().HasKey(x => x.Id);
                m.Entity<CmUser>().HasKey(x => x.Id);
            };
        });
    }
}

public class CMSEFCoreConfig
{
    public string ConnString { get; set; }
}