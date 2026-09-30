namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocumentFile;

public sealed record PreAuthDocumentFileDto(
    string StorageKey,
    string FileName,
    string ContentType);
