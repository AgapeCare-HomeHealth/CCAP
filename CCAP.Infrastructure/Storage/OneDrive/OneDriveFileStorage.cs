using CCAP.Application.Abstractions.Storage;
using Microsoft.Graph;
using Microsoft.Extensions.Options;

namespace CCAP.Infrastructure.Storage.OneDrive;

public sealed class OneDriveFileStorage : IFileStorage
{
    private readonly GraphServiceClient _graph;
    private readonly OneDriveOptions _options;

    public OneDriveFileStorage(
        GraphServiceClient graph,
        IOptions<OneDriveOptions> options)
    {
        _graph = graph;
        _options = options.Value;
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var path =
            $"{_options.RootFolder.TrimEnd('/')}/{folder.Trim('/')}/{fileName}";

        var result = await _graph
            .Drives[_options.DriveId]
            .Root
            .ItemWithPath(path)
            .Content
            .PutAsync(
                content,
                cancellationToken: cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Id))
        {
            throw new InvalidOperationException(
                "OneDrive did not return the uploaded file.");
        }

        return new StoredFile(
            result.Id,
            result.Name ?? fileName,
            contentType,
            content.CanSeek ? content.Length : 0);
    }

    public async Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var stream = await _graph
            .Drives[_options.DriveId]
            .Items[storageKey]
            .Content
            .GetAsync(
                cancellationToken: cancellationToken);

        return stream
            ?? throw new FileNotFoundException(
                "The OneDrive file could not be found.");
    }

    public async Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        await _graph
            .Drives[_options.DriveId]
            .Items[storageKey]
            .DeleteAsync(
                cancellationToken: cancellationToken);
    }
}