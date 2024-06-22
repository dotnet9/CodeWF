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

    public virtual DbSet<About> About { get; set; }
    public virtual DbSet<Category> Category { get; set; }
    public virtual DbSet<ChatGroup> ChatGroup { get; set; }
    public virtual DbSet<ChatGroupMessage> ChatGroupMessage { get; set; }
    public virtual DbSet<ChatGroupUser> ChatGroupUser { get; set; }
    public virtual DbSet<ChatUserFriend> ChatUserFriend { get; set; }
    public virtual DbSet<ChatUserMessage> ChatUserMessage { get; set; }
    public virtual DbSet<Comment> Comment { get; set; }
    public virtual DbSet<Family> Family { get; set; }
    public virtual DbSet<HistoryInfo> HistoryInfo { get; set; }
    public virtual DbSet<Post> Post { get; set; }
    public virtual DbSet<Resource> Resource { get; set; }
    public virtual DbSet<ResourcePath> ResourcePath { get; set; }
    public virtual DbSet<SystemConfig> SystemConfig { get; set; }
    public virtual DbSet<Tag> Tag { get; set; }
    public virtual DbSet<TreeHole> TreeHole { get; set; }
    public virtual DbSet<User> User { get; set; }
    public virtual DbSet<WebInfo> WebInfo { get; set; }
    public virtual DbSet<WeiYan> WeiYan { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.ApplyConfiguration(new AboutConfiguration());
    }
}

public static class BlogDbContextExtension
{
    public static async Task ClearAllData(this BlogDbContext context)
    {
        context.About.RemoveRange();

        await context.SaveChangesAsync();
    }
}