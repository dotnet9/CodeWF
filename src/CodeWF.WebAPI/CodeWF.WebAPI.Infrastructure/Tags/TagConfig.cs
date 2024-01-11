﻿namespace CodeWF.WebAPI.Infrastructure.Tags;

internal class TagConfig : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable($"{CodeWFConsts.DbTablePrefix}Tags", CodeWFConsts.DbSchema);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(TagConsts.MaxNameLength);
        builder.HasIndex(x => x.Name);
    }
}