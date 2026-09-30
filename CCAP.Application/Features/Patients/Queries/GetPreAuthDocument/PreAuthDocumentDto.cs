namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocument;

public sealed record PreAuthDocumentDto(
    Guid DocumentId,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt);
