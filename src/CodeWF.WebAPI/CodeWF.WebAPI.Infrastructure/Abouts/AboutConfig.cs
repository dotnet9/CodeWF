namespace CodeWF.WebAPI.Infrastructure.Abouts;

internal class AboutConfig : IEntityTypeConfiguration<About>
{
    public void Configure(EntityTypeBuilder<About> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Abouts", CodeWFConsts.DbSchema);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(AboutConsts.MaxContentLength);
    }
}