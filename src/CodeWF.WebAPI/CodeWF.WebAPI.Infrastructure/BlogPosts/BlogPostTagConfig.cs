namespace CodeWF.WebAPI.Infrastructure.BlogPosts;

internal class BlogPostTagConfig : IEntityTypeConfiguration<BlogPostTag>
{
    public void Configure(EntityTypeBuilder<BlogPostTag> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}BlogPostTags", CodeWFConsts.DbSchema);
        builder.HasKey(x => new { x.BlogPostId, x.TagId });
        builder.HasOne<BlogPost>().WithMany(x => x.Tags).HasForeignKey(x => x.BlogPostId).IsRequired();
        builder.HasOne<Tag>().WithMany().HasForeignKey(x => x.TagId).IsRequired();
        builder.HasIndex(x => new { x.BlogPostId, x.TagId });
    }
}