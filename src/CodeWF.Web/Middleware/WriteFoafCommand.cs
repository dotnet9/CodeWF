using System.Security.Cryptography;

namespace CodeWF.Web.Middleware;

public class WriteFoafCommand(FoafDoc doc, string currentRequestUrl, IReadOnlyList<FriendLinkEntity> links)
    : IRequest<string>
{
    public FoafDoc Doc { get; set; } = doc;

    public string CurrentRequestUrl { get; set; } = currentRequestUrl;

    public IReadOnlyList<FriendLinkEntity> Links { get; set; } = links;

    public static string ContentType => "application/rdf+xml";
}

/// <summary>
///     http://xmlns.com/foaf/spec/20140114.html
/// </summary>
public class WriteFoafCommandHandler : IRequestHandler<WriteFoafCommand, string>
{
    private static Dictionary<string, string> _xmlNamespaces;

    private static Dictionary<string, string> SupportedNamespaces =>
        _xmlNamespaces ??= new Dictionary<string, string>
        {
            { "foaf", "http://xmlns.com/foaf/0.1/" }, { "rdfs", "http://www.w3.org/2000/01/rdf-schema#" }
        };

    public async Task<string> Handle(WriteFoafCommand request, CancellationToken ct)
    {
        StringWriter sw = new();
        XmlWriter writer = await GetWriter(sw);

        await writer.WriteStartElementAsync("foaf", "PersonalProfileDocument", null);
        await writer.WriteAttributeStringAsync("rdf", "about", null, string.Empty);
        await writer.WriteStartElementAsync("foaf", "maker", null);
        await writer.WriteAttributeStringAsync("rdf", "resource", null, "#me");
        await writer.WriteEndElementAsync(); // foaf:maker
        await writer.WriteStartElementAsync("foaf", "primaryTopic", null);
        await writer.WriteAttributeStringAsync("rdf", "resource", null, "#me");
        await writer.WriteEndElementAsync(); // foaf:primaryTopic
        await writer.WriteEndElementAsync(); // foaf:PersonalProfileDocument

        FoafPerson me = new FoafPerson("#me")
        {
            Name = request.Doc.Name,
            Blog = request.Doc.BlogUrl,
            Email = request.Doc.Email,
            PhotoUrl = request.Doc.PhotoUrl,
            Friends = new List<FoafPerson>()
        };

        foreach (FriendLinkEntity friend in request.Links)
        {
            me.Friends.Add(new FoafPerson("#" + friend.Id) { Name = friend.Title, Homepage = friend.LinkUrl });
        }

        await WriteFoafPerson(writer, me, request.CurrentRequestUrl);

        await writer.WriteEndElementAsync();
        await writer.WriteEndDocumentAsync();
        writer.Close();

        await sw.FlushAsync();
        sw.GetStringBuilder();
        string xml = sw.ToString();
        return xml;
    }

    private static async Task WriteFoafPerson(XmlWriter writer, FoafPerson person, string currentRequestUrl)
    {
        await writer.WriteStartElementAsync("foaf", "Person", null);
        await writer.WriteElementStringAsync("foaf", "name", null, person.Name);
        if (person.Title != string.Empty)
        {
            await writer.WriteElementStringAsync("foaf", "title", null, person.Title);
        }

        if (person.FirstName != string.Empty)
        {
            await writer.WriteElementStringAsync("foaf", "givenname", null, person.FirstName);
        }

        if (person.LastName != string.Empty)
        {
            await writer.WriteElementStringAsync("foaf", "family_name", null, person.LastName);
        }

        if (!string.IsNullOrEmpty(person.Email))
        {
            await writer.WriteElementStringAsync("foaf", "mbox_sha1sum", null,
                CalculateSha1(person.Email, Encoding.UTF8));
        }

        if (!string.IsNullOrEmpty(person.Homepage))
        {
            await writer.WriteStartElementAsync("foaf", "homepage", null);
            await writer.WriteAttributeStringAsync("rdf", "resource", null, person.Homepage);
            await writer.WriteEndElementAsync();
        }

        if (!string.IsNullOrEmpty(person.Blog))
        {
            await writer.WriteStartElementAsync("foaf", "weblog", null);
            await writer.WriteAttributeStringAsync("rdf", "resource", null, person.Blog);
            await writer.WriteEndElementAsync();
        }

        if (person.Rdf != string.Empty && person.Rdf != currentRequestUrl)
        {
            await writer.WriteStartElementAsync("rdfs", "seeAlso", null);
            await writer.WriteAttributeStringAsync("rdf", "resource", null, person.Rdf);
            await writer.WriteEndElementAsync();
        }

        if (!string.IsNullOrEmpty(person.Birthday))
        {
            await writer.WriteElementStringAsync("foaf", "birthday", null, person.Birthday);
        }

        if (!string.IsNullOrEmpty(person.PhotoUrl))
        {
            await writer.WriteStartElementAsync("foaf", "depiction", null);
            await writer.WriteAttributeStringAsync("rdf", "resource", null, person.PhotoUrl);
            await writer.WriteEndElementAsync();
        }

        if (!string.IsNullOrEmpty(person.Phone))
        {
            await writer.WriteElementStringAsync("foaf", "phone", null, person.Phone);
        }

        if (person.Friends is { Count: > 0 })
        {
            foreach (FoafPerson friend in person.Friends)
            {
                await writer.WriteStartElementAsync("foaf", "knows", null);
                await WriteFoafPerson(writer, friend, currentRequestUrl);
                await writer.WriteEndElementAsync(); // foaf:knows
            }
        }

        await writer.WriteEndElementAsync(); // foaf:Person
    }

    private static async Task<XmlWriter> GetWriter(TextWriter sw)
    {
        XmlWriterSettings settings = new() { Encoding = Encoding.UTF8, Async = true, Indent = true };
        XmlWriter xmlWriter = XmlWriter.Create(sw, settings);

        await xmlWriter.WriteStartDocumentAsync();
        await xmlWriter.WriteStartElementAsync("rdf", "RDF", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        foreach (string prefix in SupportedNamespaces.Keys)
        {
            await xmlWriter.WriteAttributeStringAsync("xmlns", prefix, null, SupportedNamespaces[prefix]);
        }

        return xmlWriter;
    }

    private static string CalculateSha1(string text, Encoding enc)
    {
        byte[] buffer = enc.GetBytes(text);
        SHA1 cryptoTransformSha1 = SHA1.Create();
        string hash = BitConverter.ToString(cryptoTransformSha1.ComputeHash(buffer)).Replace("-", string.Empty);

        return hash.ToLower();
    }
}