using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeWF.Data.Entities;

public class SystemConfig
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public ConfigKind Kind { get; set; }
}

public enum ConfigKind
{
    Private,
    Public
}