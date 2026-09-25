using MediatR;

namespace CCAP.Application.Features.Patients.Commands.UploadPreAuthDocument;

public sealed record UploadPreAuthDocumentCommand(
    Guid PatientId,
    Guid UploadedByUserId,
    Stream Content,
    string FileName,
    string ContentType,
    long FileSize) : IRequest<UploadPreAuthDocumentResult>;
