namespace CodeWF.Email.Client;

public record PingbackNotification(
    string TargetPostTitle,
    string Domain,
    string SourceIp,
    string SourceUrl,
    string SourceTitle) : INotification;

public class PingbackNotificationHandler(ICodeWFEmailClient codeWFEmailClient, IBlogConfig blogConfig)
    : INotificationHandler<PingbackNotification>
{
    public async Task Handle(PingbackNotification notification, CancellationToken ct)
    {
        string[] dl = new[] { blogConfig.GeneralSettings.OwnerEmail };
        await codeWFEmailClient.SendEmail(MailMesageTypes.BeingPinged, dl, notification);
    }
}