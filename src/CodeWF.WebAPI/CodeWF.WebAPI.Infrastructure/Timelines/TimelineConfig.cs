namespace CodeWF.WebAPI.Infrastructure.Timelines;

internal class TimelineConfig : IEntityTypeConfiguration<Timeline>
{
    public void Configure(EntityTypeBuilder<Timeline> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Timelines", CodeWFConsts.DbSchema);
        builder.Property(x => x.Time);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(TimelineConsts.MaxTitleLength);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(TimelineConsts.MaxContentLength);
    }
}