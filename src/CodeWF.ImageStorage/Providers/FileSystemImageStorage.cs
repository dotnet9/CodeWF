using System.Runtime.InteropServices;

namespace CodeWF.ImageStorage.Providers;

public class FileSystemImageStorage(FileSystemImageConfiguration imgConfig) : IBlogImageStorage
{
    private readonly string _path = imgConfig.Path;
    public string Name => nameof(FileSystemImageStorage);

    public async Task<ImageInfo> GetAsync(string fileName)
    {
        string imagePath = Path.Join(_path, fileName);

        if (!File.Exists(imagePath))
        {
            // Can not throw FileNotFoundException,
            // because hackers may request a large number of 404 images
            // to flood .NET runtime with exceptions and take out the server
            return null;
        }

        string extension = Path.GetExtension(imagePath);

        string fileType = extension.Replace(".", string.Empty);
        byte[] imageBytes = await ReadFileAsync(imagePath);

        ImageInfo imageInfo = new ImageInfo { ImageBytes = imageBytes, ImageExtensionName = fileType };

        return imageInfo;
    }

    public async Task DeleteAsync(string fileName)
    {
        await Task.CompletedTask;
        string imagePath = Path.Join(_path, fileName);
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
        }
    }

    public async Task<string> InsertAsync(string fileName, byte[] imageBytes)
    {
        string fullPath = Path.Join(_path, fileName);

        await using FileStream sourceStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write,
            FileShare.None,
            4096, true);
        await sourceStream.WriteAsync(imageBytes.AsMemory(0, imageBytes.Length));

        return fileName;
    }

    private static async Task<byte[]> ReadFileAsync(string filename)
    {
        await using FileStream file =
            new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        byte[] buff = new byte[file.Length];
        await file.ReadAsync(buff.AsMemory(0, (int)file.Length));
        return buff;
    }

    public static string ResolveImageStoragePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        // Handle Path for non-Windows environment #412
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Path.DirectorySeparatorChar != '\\')
        {
            if (path.IndexOf('\\') > 0)
            {
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            }
        }

        // IsPathFullyQualified can't check if path is valid, e.g.:
        // Path: C:\Documents<>|foo
        //   Rooted: True
        //   Fully qualified: True
        //   Full path: C:\Documents<>|foo
        char[] invalidChars = Path.GetInvalidPathChars();
        if (path.IndexOfAny(invalidChars) >= 0)
        {
            throw new InvalidOperationException("Path can not contain invalid chars.");
        }

        if (!Path.IsPathFullyQualified(path))
        {
            throw new InvalidOperationException("Path is not fully qualified.");
        }

        string fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }
}