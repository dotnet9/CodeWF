namespace CodeWF.ImageStorage;

public class GuidFileNameGenerator(Guid id) : IFileNameGenerator
{
    public Guid UniqueId { get; } = id;
    public string Name => nameof(GuidFileNameGenerator);

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

        string newFileName =
            $"img-{UniqueId}{(string.IsNullOrWhiteSpace(appendixName) ? string.Empty : "-" + appendixName)}{ext}"
                .ToLower();
        return newFileName;
    }
}