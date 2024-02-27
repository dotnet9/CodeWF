namespace CodeWF.Data.SqlServer.Infrastructure;

public class SqlServerDbContextRepository<T>(SqlServerBlogDbContext dbContext) : DbContextRepository<T>(dbContext)
    where T : class;