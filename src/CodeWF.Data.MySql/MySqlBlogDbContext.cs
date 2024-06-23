using CodeWF.Data.MySql.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CodeWF.Data.SQLite;

public class MySqlBlogDbContext : BlogDbContext
{
    public MySqlBlogDbContext()
    {
    }

    public MySqlBlogDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AboutConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}