namespace CodeWF.Web;

public static class MemoryStreamIconGenerator
{
    public static ConcurrentDictionary<string, byte[]> SiteIconDictionary { get; set; } = new();

    public static void GenerateIcons(string base64Data, string webRootPath, ILogger logger)
    {
        byte[] buffer;

        // Fall back to default image
        if (string.IsNullOrWhiteSpace(base64Data))
        {
            logger.LogWarning("SiteIconBase64 is empty or not valid, fall back to default image.");

            // Credit: Vector Market (logo.png)
            string defaultIconImage = Path.Join($"{webRootPath}", "images", "logo.png");
            if (!File.Exists(defaultIconImage))
            {
                throw new FileNotFoundException("Can not find source image for generating favicons.", defaultIconImage);
            }

            string? ext = Path.GetExtension(defaultIconImage);
            if (ext is not null && ext.ToLower() is not ".png")
            {
                throw new FormatException("Source file is not an PNG image.");
            }

            buffer = File.ReadAllBytes(defaultIconImage);
        }
        else
        {
            buffer = Convert.FromBase64String(base64Data);
        }

        using MemoryStream ms = new MemoryStream(buffer);
        using Image image = Image.Load(ms);
        if (image.Height != image.Width)
        {
            throw new InvalidOperationException("Invalid Site Icon Data");
        }

        Dictionary<string, int[]> dic = new Dictionary<string, int[]>
        {
            { "android-icon-", new[] { 144, 192 } },
            { "favicon-", new[] { 16, 32, 96 } },
            { "apple-icon-", new[] { 180 } }
        };

        foreach ((string key, int[] value) in dic)
        {
            foreach (int size in value)
            {
                string fileName = $"{key}{size}x{size}.png";
                byte[] bytes = ResizeImage(image, size, size);

                SiteIconDictionary.TryAdd(fileName, bytes);
            }
        }

        byte[] icon1Bytes = ResizeImage(image, 180, 180);
        SiteIconDictionary.TryAdd("apple-icon.png", icon1Bytes);
    }

    public static byte[] GetIcon(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return SiteIconDictionary.GetValueOrDefault(fileName);
    }

    private static byte[] ResizeImage(Image image, int toWidth, int toHeight)
    {
        image.Mutate(x => x.Resize(toWidth, toHeight));
        using MemoryStream ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }
}