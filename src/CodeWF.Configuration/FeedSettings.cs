namespace CodeWF.Configuration;

public class FeedSettings : IBlogSettings
{
    [Display(Name = "RSS items")] public int RssItemCount { get; set; }

    [Display(Name = "Use full blog post content instead of abstract")]
    public bool UseFullContent { get; set; }

    [JsonIgnore] public static FeedSettings DefaultValue => new() { RssItemCount = 20, UseFullContent = false };
}