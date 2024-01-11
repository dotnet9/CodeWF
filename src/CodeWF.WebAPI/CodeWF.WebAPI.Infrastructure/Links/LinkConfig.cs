namespace CodeWF.WebAPI.Infrastructure.Links;

internal class LinkConfig : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Links", CodeWFConsts.DbSchema);
        builder.Property(x => x.SequenceNumber);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(LinkConsts.MaxNameLength);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(LinkConsts.MaxUrlLength);
        builder.Property(x => x.Description).HasMaxLength(LinkConsts.MaxDescriptionLength);
        builder.Property(x => x.Kind);
    }
}