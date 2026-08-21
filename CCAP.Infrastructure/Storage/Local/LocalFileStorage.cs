using CCAP.Application.Abstractions.Storage;
using CCAP.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace CCAP.Infrastructure.Storage.Local;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(
        IOptions<FileStorageOptions> options)
    {
        var configuredPath =
            options.Value.LocalRootPath;

        _rootPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(
                AppContext.BaseDirectory,
                configuredPath);
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var safeFolder = folder
            .Replace(
                "/",
                Path.DirectorySeparatorChar.ToString())
            .Replace(
                "\\",
                Path.DirectorySeparatorChar.ToString());

        var directory = Path.Combine(
            _rootPath,
            safeFolder);

        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(
            directory,
            fileName);

        await using var output = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await content.CopyToAsync(
            output,
            cancellationToken);

        var size =
            new FileInfo(fullPath).Length;

        return new StoredFile(
            fullPath,
            fileName,
            contentType,
            size);
    }

    public Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        Stream stream = new FileStream(
            storageKey,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        if (File.Exists(storageKey))
        {
            File.Delete(storageKey);
        }

        return Task.CompletedTask;
    }
}