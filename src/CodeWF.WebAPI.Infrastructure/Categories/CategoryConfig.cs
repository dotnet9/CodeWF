namespace CodeWF.WebAPI.Infrastructure.Categories;

internal class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Categories", CodeWFConsts.DbSchema);
        builder.Property(x => x.SequenceNumber);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(CategoryConsts.MaxNameLength);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(CategoryConsts.MaxSlugLength);
        builder.Property(x => x.Cover).IsRequired().HasMaxLength(CategoryConsts.MaxCoverLength);
        builder.Property(x => x.Description).HasMaxLength(CategoryConsts.MaxDescriptionLength);
        builder.Property(x => x.ParentId);
        builder.Property(x => x.Visible);
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Slug);
    }
}