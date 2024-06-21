namespace CodeWF.Data.Entities;

public class ChatUserMessage
{
    public Guid Id { get; set; }

    public Guid FromId { get; set; }

    public Guid ToId { get; set; }

    public string Content { get; set; } = null!;

    public EnabledKind MessageStatus { get; set; }

    public DateTime CreateTime { get; set; }
}