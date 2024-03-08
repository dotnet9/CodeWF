namespace CodeWF.Data.SQLite.Configurations;

internal class BlogConfigurationConfiguration : IEntityTypeConfiguration<BlogConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<BlogConfigurationEntity> builder)
    {
        builder.Property(e => e.LastModifiedTimeUtc).HasColumnType("datetime");
    }
}