namespace CodeWF.Data.MySql.Configurations;

internal class PageConfiguration : IEntityTypeConfiguration<PageEntity>
{
    public void Configure(EntityTypeBuilder<PageEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Title).HasMaxLength(PostConsts.MaxTitleLength);
        builder.Property(e => e.Slug).HasMaxLength(PostConsts.MaxSlugLength);
        builder.Property(e => e.CssId).HasMaxLength(64);
        builder.Property(e => e.MetaDescription).HasMaxLength(PostConsts.MaxDescriptionLength);
        builder.Property(e => e.CreateTimeUtc).HasColumnType("datetime");
        builder.Property(e => e.UpdateTimeUtc).HasColumnType("datetime");
    }
}