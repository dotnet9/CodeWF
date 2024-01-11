namespace CodeWF.WebAPI.Infrastructure;

public class CodeWFDbContext : IdentityDbContext<User, Role, Guid>
{
    public CodeWFDbContext(DbContextOptions<CodeWFDbContext> options)
        : base(options)
    {
    }

    public DbSet<About>? Abouts { get; private set; }
    public DbSet<ActionLog>? ActionLogs { get; private set; }
    public DbSet<Category>? Categories { get; private set; }
    public DbSet<Album>? Albums { get; private set; }
    public DbSet<Tag>? Tags { get; private set; }
    public DbSet<BlogPost>? BlogPosts { get; private set; }
    public DbSet<Donation>? Donations { get; private set; }
    public DbSet<Privacy>? Privacies { get; private set; }
    public DbSet<Link>? Links { get; private set; }
    public DbSet<Timeline>? Timelines { get; private set; }
    public DbSet<Comment>? Comments { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.EnableSoftDeletionGlobalFilter();
    }
}