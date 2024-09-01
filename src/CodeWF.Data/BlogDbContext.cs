using CodeWF.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeWF.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext()
    {
    }

    public BlogDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<AboutEntity> About { get; set; }
    public virtual DbSet<CategoryEntity> Category { get; set; }
    public virtual DbSet<CommentEntity> Comment { get; set; }
    public virtual DbSet<PostEntity> Post { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
    }
}

public static class BlogDbContextExtension
{
    public static async Task ClearAllData(this BlogDbContext context)
    {
        context.About.RemoveRange();
        context.Category.RemoveRange();

        await context.SaveChangesAsync();
    }
}