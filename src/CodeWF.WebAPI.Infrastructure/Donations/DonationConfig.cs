namespace CodeWF.WebAPI.Infrastructure.Donations;

internal class DonationConfig : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Donations", CodeWFConsts.DbSchema);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(DonationConsts.MaxContentLength);
    }
}