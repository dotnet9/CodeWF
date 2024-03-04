namespace CodeWF.Tools.Module.Test.MediatR.NotificationHandlers;

public class TestNotificationHandler(INotificationService notificationService) : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        notificationService.Show("Notification",
            $"模块【Test】Notification处理程序：Args = {notification.Args}, Now = {DateTime.Now}");
        return Task.CompletedTask;
    }
}