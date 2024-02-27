namespace CodeWF.Pingback;

public interface IPingbackSender
{
    Task TrySendPingAsync(string postUrl, string postContent);
}