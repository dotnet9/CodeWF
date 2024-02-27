namespace CodeWF.Web.Middleware;

// Really Simple Discovery (RSD) is a protocol or method that makes it easier for client software to automatically discover the API endpoint needed to interact with a web service. It's primarily used in the context of blog software and content management systems (CMS).
// 
// RSD allows client applications, like blog editors and content aggregators, to find the services needed to read, edit, or work with the content of a website without the user having to manually input the details of the API endpoints. For example, if you're using a desktop blogging application, RSD would enable that application to find the endpoint for the XML-RPC API of your blog so you can post directly from your desktop.

public class RSDMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext, IBlogConfig blogConfig)
    {
        if (httpContext.Request.Path == "/rsd")
        {
            string siteRootUrl = Helper.ResolveRootUrl(httpContext, blogConfig.GeneralSettings.CanonicalPrefix, true);
            string xml = await GetRSDData(siteRootUrl);

            httpContext.Response.ContentType = "text/xml";
            await httpContext.Response.WriteAsync(xml, httpContext.RequestAborted);
        }
        else
        {
            await next(httpContext);
        }
    }

    private static async Task<string> GetRSDData(string siteRootUrl)
    {
        StringBuilder sb = new StringBuilder();

        XmlWriterSettings writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, Async = true };
        await using (XmlWriter writer = XmlWriter.Create(sb, writerSettings))
        {
            await writer.WriteStartDocumentAsync();

            // Rsd tag
            writer.WriteStartElement("rsd");
            writer.WriteAttributeString("version", "1.0");

            // Service 
            writer.WriteStartElement("service");
            writer.WriteElementString("engineName", $"CodeWF {Helper.AppVersion}");
            writer.WriteElementString("engineLink", "https://github.com/dotnet9/CodeWF");
            writer.WriteElementString("homePageLink", siteRootUrl);

            // APIs
            writer.WriteStartElement("apis");

            // MetaWeblog
            writer.WriteStartElement("api");
            writer.WriteAttributeString("name", "MetaWeblog");
            writer.WriteAttributeString("preferred", "true");
            writer.WriteAttributeString("apiLink", $"{siteRootUrl}metaweblog");
            writer.WriteAttributeString("blogID", siteRootUrl);
            await writer.WriteEndElementAsync();

            // End APIs
            await writer.WriteEndElementAsync();

            // End Service
            await writer.WriteEndElementAsync();

            // End Rsd
            await writer.WriteEndElementAsync();

            await writer.WriteEndDocumentAsync();
        }

        string xml = sb.ToString();
        return xml;
    }
}