using CodeWF.Data.SQLite.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CodeWF.Data.SQLite;

public class SQLiteBlogDbContext : BlogDbContext
{
    public SQLiteBlogDbContext()
    {
    }

    public SQLiteBlogDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AboutConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}