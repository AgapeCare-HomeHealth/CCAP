namespace CCAP.Application.Abstractions.Storage;

public sealed record StoredFileResult(
    string FileId,
    string FileName,
    string? WebUrl);