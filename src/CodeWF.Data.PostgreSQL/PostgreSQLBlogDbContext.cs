using CodeWF.Data.PostgreSQL.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CodeWF.Data.PostgreSQL;

public class PostgreSQLBlogDbContext : BlogDbContext
{
    public PostgreSQLBlogDbContext()
    {
    }

    public PostgreSQLBlogDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AboutConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}