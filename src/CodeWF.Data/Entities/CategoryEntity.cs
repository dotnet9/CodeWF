using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace CodeWF.Data.Entities;

public class CategoryEntity
{
    public CategoryEntity()
    {
        PostCategory = new HashSet<PostCategoryEntity>();
    }

    public Guid Id { get; set; }
    public int Sort { get; set; }
    public string Slug { get; set; }
    public string DisplayName { get; set; }
    public string Note { get; set; }

    [JsonIgnore] public virtual ICollection<PostCategoryEntity> PostCategory { get; set; }
}

internal class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.DisplayName).HasMaxLength(64);
        builder.Property(e => e.Note).HasMaxLength(128);
        builder.Property(e => e.Slug).HasMaxLength(64);
    }
}