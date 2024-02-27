namespace CodeWF.Email.Client;

public record TestNotification : INotification;

public class TestNotificationHandler(ICodeWFEmailClient codeWFEmailClient, IBlogConfig blogConfig)
    : INotificationHandler<TestNotification>
{
    public async Task Handle(TestNotification notification, CancellationToken ct)
    {
        string[] dl = new[] { blogConfig.GeneralSettings.OwnerEmail };
        await codeWFEmailClient.SendEmail(MailMesageTypes.TestMail, dl, EmptyPayload.Default);
    }
}