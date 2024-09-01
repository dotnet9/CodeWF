using CodeWF.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWF.Data.SQLite.Configurations;

internal class AboutConfiguration : IEntityTypeConfiguration<AboutEntity>
{
    public void Configure(EntityTypeBuilder<AboutEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Title).HasMaxLength(128);
        builder.Property(e => e.Content).HasMaxLength(10 * 1024);
        builder.Property(e => e.UpdateTime);
    }
}