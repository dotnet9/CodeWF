namespace CodeWF.WebAPI.Infrastructure.Albums;

internal class AlbumCategoryConfig : IEntityTypeConfiguration<AlbumCategory>
{
    public void Configure(EntityTypeBuilder<AlbumCategory> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}AlbumCategories", CodeWFConsts.DbSchema);
        builder.HasKey(x => new { x.AlbumId, x.CategoryId });
        builder.HasOne<Album>().WithMany(x => x.Categories).HasForeignKey(x => x.AlbumId).IsRequired();
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).IsRequired();
        builder.HasIndex(x => new { x.AlbumId, x.CategoryId });
    }
}