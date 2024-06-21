namespace CodeWF.Data.Entities;

public class ChatGroup
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid MasterUserId { get; set; }

    public string? Avatar { get; set; }

    public string? Introduction { get; set; }

    public string? Notice { get; set; }

    public ChatGroupInKind InType { get; set; }

    public ChatGroupKind GroupType { get; set; }

    public DateTime CreateTime { get; set; }
}

public enum ChatGroupInKind
{
    All,
    Agree
}

public enum ChatGroupKind
{
    ChatGroup,
    Topic
}