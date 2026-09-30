using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CCAP.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace CCAP.Infrastructure.Storage.AzureBlob;

/// <summary>
/// Azure Blob implementation prepared for hosted deployment. It is not
/// registered while FileStorage:Provider is MetadataOnly.
/// </summary>
public sealed class AzureBlobFileStorage : IFileStorage
{
    public bool CanStore => true;

    private readonly BlobContainerClient _container;
    private readonly AzureBlobOptions _options;

    public AzureBlobFileStorage(IOptions<AzureBlobOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("AzureBlob:ConnectionString is required when AzureBlob storage is enabled.");

        _container = new BlobContainerClient(
            _options.ConnectionString,
            _options.ContainerName);
    }

    public async Task<StoredFile> SaveAsync(
        Stream content, string fileName, string contentType, string folder,
        CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var safeName = Path.GetFileName(fileName);
        var safeFolder = folder.Replace('\\', '/').Trim('/');
        var root = _options.RootFolder.Trim('/');
        var blobName = string.Join('/', new[] { root, safeFolder, $"{Guid.NewGuid():N}-{safeName}" }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        var blob = _container.GetBlobClient(blobName);
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blob.UploadAsync(content, options, cancellationToken);
        var length = content.CanSeek ? content.Length : 0;

        return new StoredFile(blobName, safeName, contentType, length);
    }

    public async Task<Stream> OpenReadAsync(
        string storageKey, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(storageKey);
        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public Task DeleteAsync(
        string storageKey, CancellationToken cancellationToken = default) =>
        _container.DeleteBlobIfExistsAsync(storageKey, cancellationToken: cancellationToken);
}
