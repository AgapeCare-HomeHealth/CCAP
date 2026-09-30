using CCAP.Application.Abstractions.Storage;

namespace CCAP.Infrastructure.Storage;

/// <summary>
/// Used while file persistence is intentionally disabled. Metadata can still
/// be stored in the database; file operations fail explicitly if invoked.
/// </summary>
public sealed class NullFileStorage : IFileStorage
{
    public bool CanStore => false;

    public Task<StoredFile> SaveAsync(
        Stream content, string fileName, string contentType, string folder,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "File storage is disabled. Only file metadata is currently persisted.");

    public Task<Stream> OpenReadAsync(
        string storageKey, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "File storage is disabled. PDF content is not currently available.");

    public Task DeleteAsync(
        string storageKey, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "File storage is disabled. PDF content is not currently available.");
}
