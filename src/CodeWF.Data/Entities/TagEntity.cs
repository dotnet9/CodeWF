using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CodeWF.Data.Entities;

public class TagEntity
{
    public TagEntity()
    {
        Posts = new HashSet<PostEntity>();
    }

    public int Id { get; set; }

    [MaxLength(32)] public string DisplayName { get; set; }

    [MaxLength(32)] public string NormalizedName { get; set; }

    [JsonIgnore] public virtual ICollection<PostEntity> Posts { get; set; }
}