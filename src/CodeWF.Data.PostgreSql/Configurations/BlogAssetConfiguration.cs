namespace CodeWF.Data.PostgreSql.Configurations;

internal class BlogAssetConfiguration : IEntityTypeConfiguration<BlogAssetEntity>
{
    public void Configure(EntityTypeBuilder<BlogAssetEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.LastModifiedTimeUtc).HasColumnType("timestamp");
    }
}