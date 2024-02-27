using System.Globalization;

namespace CodeWF.Syndication;

public class FeedGenerator : IFeedGenerator, IRssGenerator, IAtomGenerator
{
    public FeedGenerator()
    {
        FeedItemCollection = new List<FeedEntry>();
    }

    public FeedGenerator(string hostUrl, string headTitle, string headDescription, string copyright, string generator,
        string trackBackUrl, string language)
    {
        HostUrl = hostUrl;
        HeadTitle = headTitle;
        HeadDescription = headDescription;
        Copyright = copyright;
        Generator = generator;
        TrackBackUrl = trackBackUrl;
        Language = language;

        FeedItemCollection = new List<FeedEntry>();
    }

    public async Task<string> WriteAtomAsync()
    {
        IEnumerable<SyndicationItem> feed = GetItemCollection(FeedItemCollection);

        StringWriterWithEncoding sw = new(Encoding.UTF8);
        await using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true }))
        {
            AtomFeedWriter writer = new(xmlWriter);

            await writer.WriteTitle(HeadTitle);
            await writer.WriteSubtitle(HeadDescription);
            await writer.WriteRights(Copyright);
            await writer.WriteUpdated(DateTime.UtcNow);
            await writer.WriteGenerator(Generator, HostUrl, GeneratorVersion);

            foreach (SyndicationItem item in feed)
            {
                await writer.Write(item);
            }

            await xmlWriter.FlushAsync();
            xmlWriter.Close();
        }

        string xml = sw.ToString();
        return xml;
    }

    public async Task<string> WriteRssAsync()
    {
        IEnumerable<SyndicationItem> feed = GetItemCollection(FeedItemCollection);

        StringWriterWithEncoding sw = new(Encoding.UTF8);
        await using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true }))
        {
            RssFeedWriter writer = new(xmlWriter);

            await writer.WriteTitle(HeadTitle);
            await writer.WriteDescription(HeadDescription);
            await writer.Write(new SyndicationLink(new Uri(TrackBackUrl)));
            await writer.WritePubDate(DateTimeOffset.UtcNow);
            await writer.WriteCopyright(Copyright);
            await writer.WriteGenerator(Generator);
            await writer.WriteLanguage(new CultureInfo(Language));

            foreach (SyndicationItem item in feed)
            {
                await writer.Write(item);
            }

            await xmlWriter.FlushAsync();
            xmlWriter.Close();
        }

        string xml = sw.ToString();
        return xml;
    }

    private static IEnumerable<SyndicationItem> GetItemCollection(IEnumerable<FeedEntry> itemCollection)
    {
        List<SyndicationItem> synItemCollection = new();
        if (null == itemCollection)
        {
            return synItemCollection;
        }

        foreach (FeedEntry item in itemCollection)
        {
            // create rss item
            SyndicationItem sItem = new()
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                LastUpdated = item.PubDateUtc.ToUniversalTime(),
                Published = item.PubDateUtc.ToUniversalTime()
            };

            sItem.AddLink(new SyndicationLink(new Uri(item.Link)));

            // add author
            if (!string.IsNullOrWhiteSpace(item.Author) && !string.IsNullOrWhiteSpace(item.AuthorEmail))
            {
                sItem.AddContributor(new SyndicationPerson(item.Author, item.AuthorEmail));
            }

            // add categories
            if (item.Categories is { Length: > 0 })
            {
                foreach (string itemCategory in item.Categories)
                {
                    sItem.AddCategory(new SyndicationCategory(itemCategory));
                }
            }

            synItemCollection.Add(sItem);
        }

        return synItemCollection;
    }

    #region Properties

    public IEnumerable<FeedEntry> FeedItemCollection { get; set; }
    public string HostUrl { get; set; }
    public string HeadTitle { get; set; }
    public string HeadDescription { get; set; }
    public string Copyright { get; set; }
    public string Generator { get; set; }
    public string TrackBackUrl { get; set; }
    public string GeneratorVersion { get; set; }
    public string Language { get; set; }

    #endregion
}

public class StringWriterWithEncoding(Encoding encoding) : StringWriter
{
    public override Encoding Encoding { get; } = encoding;
}