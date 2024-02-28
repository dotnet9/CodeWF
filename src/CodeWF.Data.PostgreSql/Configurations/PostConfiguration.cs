using CodeWF.Data.Consts;

namespace CodeWF.Data.PostgreSql.Configurations;

internal class PostConfiguration : IEntityTypeConfiguration<PostEntity>
{
    public void Configure(EntityTypeBuilder<PostEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.CommentEnabled);
        builder.Property(e => e.ContentAbstract).HasMaxLength(1024);
        builder.Property(e => e.ContentLanguageCode).HasMaxLength(8);

        builder.Property(e => e.CreateTimeUtc).HasColumnType("timestamp");
        builder.Property(e => e.PubDateUtc).HasColumnType("timestamp");
        builder.Property(e => e.LastModifiedUtc).HasColumnType("timestamp");
        builder.Property(e => e.PostContent);

        builder.Property(e => e.Author).HasMaxLength(64);
        builder.Property(e => e.Slug).HasMaxLength(PostConsts.MaxSlugLength);
        builder.Property(e => e.Title).HasMaxLength(PostConsts.MaxTitleLength);
        builder.Property(e => e.OriginLink).HasMaxLength(PostConsts.MaxOriginalTitleLength);
        builder.Property(e => e.HeroImageUrl).HasMaxLength(PostConsts.MaxCoverLength);
    }
}