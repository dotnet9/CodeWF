namespace CodeWF.Data.Entities;

public class HistoryInfo
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Ip { get; set; } = null!;

    public string? Nation { get; set; }

    public string? Province { get; set; }

    public string? City { get; set; }

    public DateTime CreateTime { get; set; }
}