namespace CodeWF.Tools.Module.Test.ViewModels;

public class TestViewModel : ViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly ISender _sender;
    private readonly IPublisher _publisher;
    private readonly IEventAggregator _eventAggregator;

    public TestViewModel(INotificationService notificationService,
        ISender sender, IPublisher publisher,
        IEventAggregator eventAggregator)
    {
        _notificationService = notificationService;
        _sender = sender;
        _publisher = publisher;
        _eventAggregator = eventAggregator;

        RegisterPrismEvent();
    }

    private void RegisterPrismEvent()
    {
        _eventAggregator.GetEvent<TestEvent>().Subscribe(args =>
        {
            _notificationService?.Show("Prism Event",
                $"模块【Test】Prism事件处理程序：Args = {args.Args}, Now = {DateTime.Now}");
        });
    }

    public async Task ExecuteMediatRRequestAsync()
    {
        var result = _sender.Send(new TestRequest() { Args = "ExecuteMediatRRequestAsync" });
        _notificationService.Show("MediatR", $"收到响应：{result.Result}");
        await Task.CompletedTask;
    }

    public async Task ExecuteMediatRNotificationAsync()
    {
        await _publisher.Publish(new TestNotification() { Args = "ExecuteMediatRNotificationAsync" });
    }

    public Task ExecutePrismEventAsync()
    {
        var prismEvent = _eventAggregator.GetEvent<TestEvent>();
        prismEvent.Publish(new TestEventParameter() { Args = "ExecutePrismEventAsync" });
        return Task.CompletedTask;
    }
}