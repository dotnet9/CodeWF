namespace CodeWF.WebAPI.Infrastructure.Privacies;

internal class PrivacyConfig : IEntityTypeConfiguration<Privacy>
{
    public void Configure(EntityTypeBuilder<Privacy> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Privacies", CodeWFConsts.DbSchema);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(PrivacyConsts.MaxContentLength);
    }
}