namespace CodeWF.Data.SQLite.Infrastructure;

public class SQLiteDbContextRepository<T>(SQLiteBlogDbContext dbContext) : DbContextRepository<T>(dbContext)
    where T : class;