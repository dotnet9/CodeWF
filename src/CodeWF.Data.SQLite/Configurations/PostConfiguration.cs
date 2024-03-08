namespace CodeWF.Data.SQLite.Configurations;

internal class PostConfiguration : IEntityTypeConfiguration<PostEntity>
{
    public void Configure(EntityTypeBuilder<PostEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.CommentEnabled);
        builder.Property(e => e.ContentAbstract).HasMaxLength(1024);
        builder.Property(e => e.ContentLanguageCode).HasMaxLength(8);

        builder.Property(e => e.CreateTimeUtc).HasColumnType("datetime");
        builder.Property(e => e.PubDateUtc).HasColumnType("datetime");
        builder.Property(e => e.LastModifiedUtc).HasColumnType("datetime");
        builder.Property(e => e.PostContent);

        builder.Property(e => e.Author).HasMaxLength(64);
        builder.Property(e => e.Slug).HasMaxLength(PostConsts.MaxSlugLength);
        builder.Property(e => e.Title).HasMaxLength(PostConsts.MaxTitleLength);
        builder.Property(e => e.OriginLink).HasMaxLength(PostConsts.MaxOriginalLinkLength);
        builder.Property(e => e.HeroImageUrl).HasMaxLength(PostConsts.MaxCoverLength);
    }
}