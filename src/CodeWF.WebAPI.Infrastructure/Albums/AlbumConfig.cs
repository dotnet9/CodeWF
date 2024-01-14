namespace CodeWF.WebAPI.Infrastructure.Albums;

internal class AlbumConfig : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Albums", CodeWFConsts.DbSchema);
        builder.Property(x => x.SequenceNumber);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AlbumConsts.MaxNameLength);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(AlbumConsts.MaxSlugLength);
        builder.Property(x => x.Cover).IsRequired().HasMaxLength(AlbumConsts.MaxCoverLength);
        builder.Property(x => x.Description).HasMaxLength(AlbumConsts.MaxDescriptionLength);
        builder.Property(x => x.Visible);
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Slug);
    }
}