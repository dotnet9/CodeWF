namespace CodeWF.Data.Entities;

public class ChatGroupMessage
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }

    public Guid FromId { get; set; }

    public Guid ToId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreateTime { get; set; }
}