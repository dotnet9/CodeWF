namespace CodeWF.Models;

public class ActivityDetail
{
    public ActivityDetail(string cover, string mobileCover)
    {
        Cover = cover;
        MobileCover = mobileCover;
    }

    public ActivityDetail(string? video, string cover, string mobileCover, string? file) : this(cover, mobileCover)
    {
        Video = video;
        File = file;
    }

    public string? Video { get; init; }
    public string? Cover { get; init; }
    public string? MobileCover { get; init; }
    public string? File { get; init; }
}
