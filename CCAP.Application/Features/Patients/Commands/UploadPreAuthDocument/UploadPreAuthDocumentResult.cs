namespace CCAP.Application.Features.Patients.Commands.UploadPreAuthDocument;

public sealed record UploadPreAuthDocumentResult(
    Guid DocumentId,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt);
