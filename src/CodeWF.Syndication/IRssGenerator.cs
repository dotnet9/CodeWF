namespace CodeWF.Syndication;

public interface IRssGenerator
{
    Task<string> WriteRssAsync();
}