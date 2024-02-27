namespace CodeWF.Email.Client;

public record CommentReplyNotification(
    string Email,
    string CommentContent,
    string Title,
    string ReplyContentHtml,
    string PostLink) : INotification;

public class CommentReplyNotificationHandler(ICodeWFEmailClient codeWFEmailClient)
    : INotificationHandler<CommentReplyNotification>
{
    public async Task Handle(CommentReplyNotification notification, CancellationToken ct)
    {
        string[] dl = new[] { notification.Email };
        await codeWFEmailClient.SendEmail(MailMesageTypes.AdminReplyNotification, dl, notification);
    }
}