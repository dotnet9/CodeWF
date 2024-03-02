namespace CodeWF.Tools.Desktop.MediatR.NotificationHandlers;

public class TestNotificationHandler(INotificationService notificationService) : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        notificationService.Show("Notification",
            $"主工程Notification处理程序：Args = {notification.Args}, Now = {DateTime.Now}");
        return Task.CompletedTask;
    }
}