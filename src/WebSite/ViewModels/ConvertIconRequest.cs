namespace WebSite.ViewModels;

public class ConvertIconRequest
{
    public IFormFile SourceImage { get; set; }
    public uint[] ConvertSizes { get; set; }
}