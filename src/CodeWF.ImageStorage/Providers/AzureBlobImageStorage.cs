using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace CodeWF.ImageStorage.Providers;

public class AzureBlobImageStorage : IBlogImageStorage
{
    private readonly BlobContainerClient _container;

    private readonly ILogger<AzureBlobImageStorage> _logger;

    public AzureBlobImageStorage(ILogger<AzureBlobImageStorage> logger, AzureBlobConfiguration blobConfiguration)
    {
        _logger = logger;

        _container = new BlobContainerClient(blobConfiguration.ConnectionString, blobConfiguration.ContainerName);

        logger.LogInformation(
            $"Created {nameof(AzureBlobImageStorage)} for account {_container.AccountName} on container {_container.Name}");
    }

    public string Name => nameof(AzureBlobImageStorage);

    public async Task<string> InsertAsync(string fileName, byte[] imageBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        _logger.LogInformation($"Uploading {fileName} to Azure Blob Storage.");


        BlobClient? blob = _container.GetBlobClient(fileName);

        // Why .NET doesn't have MimeMapping.GetMimeMapping()
        BlobHttpHeaders blobHttpHeader = new BlobHttpHeaders();
        string extension = Path.GetExtension(blob.Uri.AbsoluteUri);
        blobHttpHeader.ContentType = extension.ToLower() switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => blobHttpHeader.ContentType
        };

        await using MemoryStream fileStream = new MemoryStream(imageBytes);
        Response<BlobContentInfo>? uploadedBlob = await blob.UploadAsync(fileStream, blobHttpHeader);

        _logger.LogInformation(
            $"Uploaded image file '{fileName}' to Azure Blob Storage, ETag '{uploadedBlob.Value.ETag}'. Yeah, the best cloud!");

        return fileName;
    }

    public async Task DeleteAsync(string fileName)
    {
        await _container.DeleteBlobIfExistsAsync(fileName);
    }

    public async Task<ImageInfo> GetAsync(string fileName)
    {
        BlobClient? blobClient = _container.GetBlobClient(fileName);
        await using MemoryStream memoryStream = new MemoryStream();
        string extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("File extension is empty");
        }

        Task<Response<bool>>? existsTask = blobClient.ExistsAsync();
        Task<Response>? downloadTask = blobClient.DownloadToAsync(memoryStream);

        Response<bool>? exists = await existsTask;
        if (!exists)
        {
            _logger.LogWarning($"Blob {fileName} not exist.");

            // Can not throw FileNotFoundException,
            // because hackers may request a large number of 404 images
            // to flood .NET runtime with exceptions and take out the server
            return null;
        }

        await downloadTask;
        byte[] arr = memoryStream.ToArray();

        string fileType = extension.Replace(".", string.Empty);
        ImageInfo imageInfo = new ImageInfo { ImageBytes = arr, ImageExtensionName = fileType };

        return imageInfo;
    }
}