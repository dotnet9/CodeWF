namespace CodeWF.Data.Entities;

public class ChatGroupUser
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }

    public Guid UserId { get; set; }

    public Guid VerifyUserId { get; set; }

    public string? Remark { get; set; }

    public EnabledKind AdminFlag { get; set; }

    public ChatGroupUserKind UserStatus { get; set; }

    public DateTime CreateTime { get; set; }
}

public enum ChatGroupUserKind
{
    Unaudited,
    Approved,
    Prohibition
}