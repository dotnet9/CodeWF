namespace CodeWF.ImageStorage.Providers;

public class FileSystemImageConfiguration(string path)
{
    public string Path { get; set; } = path;
}