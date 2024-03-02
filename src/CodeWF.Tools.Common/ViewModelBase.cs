namespace CodeWF.Tools.Common;

public class ViewModelBase(ISender sender, IPublisher publisher) : BindableBase
{
    public ISender Sender { get; } = sender;
    public IPublisher Publisher { get; } = publisher;
}