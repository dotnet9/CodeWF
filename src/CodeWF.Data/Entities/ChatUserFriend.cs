namespace CodeWF.Data.Entities;

public class ChatUserFriend
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid FriendId { get; set; }

    public EnabledKind Status { get; set; }

    public string? Remark { get; set; }

    public DateTime CreateTime { get; set; }
}