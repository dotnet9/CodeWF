namespace CodeWF.Data.MySql.Infrastructure;

public class MySqlDbContextRepository<T>(MySqlBlogDbContext dbContext) : DbContextRepository<T>(dbContext)
    where T : class;