namespace CodeWF.Email.Client;

public record CommentNotification(
    string Username,
    string Email,
    string IPAddress,
    string PostTitle,
    string CommentContent) : INotification;

public class CommentNotificationHandler(ICodeWFEmailClient codeWFEmailClient, IBlogConfig blogConfig)
    : INotificationHandler<CommentNotification>
{
    public async Task Handle(CommentNotification notification, CancellationToken ct)
    {
        notification = notification with
        {
            CommentContent = ContentProcessor.MarkdownToContent(notification.CommentContent,
                ContentProcessor.MarkdownConvertType.Html)
        };

        string[] dl = new[] { blogConfig.GeneralSettings.OwnerEmail };
        await codeWFEmailClient.SendEmail(MailMesageTypes.NewCommentNotification, dl, notification);
    }
}