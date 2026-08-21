namespace CCAP.Application.Abstractions.Storage;

public sealed record StoredFile(
    string StorageKey,
    string OriginalFileName,
    string ContentType,
    long Size);