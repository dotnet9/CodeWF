namespace CodeWF.Data.SqlServer.Configurations;

public class BlogThemeConfiguration : IEntityTypeConfiguration<BlogThemeEntity>
{
    public void Configure(EntityTypeBuilder<BlogThemeEntity> builder)
    {
        builder.Property(e => e.Id).UseIdentityColumn();
        builder.Property(e => e.ThemeName).HasMaxLength(32);
    }
}