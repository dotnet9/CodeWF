namespace CodeWF.WebAPI.Infrastructure.BlogPosts;

internal class BlogPostCategoryConfig : IEntityTypeConfiguration<BlogPostCategory>
{
    public void Configure(EntityTypeBuilder<BlogPostCategory> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}BlogPostCategories", CodeWFConsts.DbSchema);
        builder.HasKey(x => new { x.BlogPostId, x.CategoryId });
        builder.HasOne<BlogPost>().WithMany(x => x.Categories).HasForeignKey(x => x.BlogPostId).IsRequired();
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).IsRequired();
        builder.HasIndex(x => new { x.BlogPostId, x.CategoryId });
    }
}