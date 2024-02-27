namespace CodeWF.ImageStorage;

public class RegularFileNameGenerator : IFileNameGenerator
{
    public string Name => nameof(RegularFileNameGenerator);

    public string GetFileName(string fileName, string appendixName = "")
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        string ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) ||
            string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(fileName)))
        {
            throw new ArgumentException("Invalid File Name", nameof(fileName));
        }

        string uniqueId = DateTime.Now.ToString("yyMMdd") + Guid.NewGuid().ToString("N")[..6];

        string newFileName =
            $"img-{uniqueId}{(string.IsNullOrWhiteSpace(appendixName) ? string.Empty : "-" + appendixName)}{ext}"
                .ToLower();
        return newFileName;
    }
}