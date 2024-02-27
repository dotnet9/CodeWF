namespace CodeWF.Data.PostgreSql.Infrastructure;

public class PostgreSqlDbContextRepository<T>(PostgreSqlBlogDbContext dbContext) : DbContextRepository<T>(dbContext)
    where T : class;