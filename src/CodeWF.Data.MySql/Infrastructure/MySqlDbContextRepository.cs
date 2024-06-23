using CodeWF.Data.SQLite;

namespace CodeWF.Data.MySql.Infrastructure;

public class MySqlDbContextRepository<T>(MySqlBlogDbContext dbContext)
    : CodeWFRepository<T>(dbContext) where T : class;