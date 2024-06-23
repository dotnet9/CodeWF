namespace CodeWF.Data.PostgreSQL.Infrastructure;

public class PostgreSQLDbContextRepository<T>(PostgreSQLBlogDbContext dbContext)
    : CodeWFRepository<T>(dbContext) where T : class;