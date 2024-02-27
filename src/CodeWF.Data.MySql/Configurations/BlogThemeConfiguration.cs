namespace CodeWF.Data.MySql.Configurations;

public class BlogThemeConfiguration : IEntityTypeConfiguration<BlogThemeEntity>
{
    public void Configure(EntityTypeBuilder<BlogThemeEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ThemeName).HasMaxLength(32);
    }
}