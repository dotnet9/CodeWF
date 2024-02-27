namespace CodeWF.Data.PostgreSql.Configurations;

internal class BlogConfigurationConfiguration : IEntityTypeConfiguration<BlogConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<BlogConfigurationEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.LastModifiedTimeUtc).HasColumnType("timestamp");
    }
}