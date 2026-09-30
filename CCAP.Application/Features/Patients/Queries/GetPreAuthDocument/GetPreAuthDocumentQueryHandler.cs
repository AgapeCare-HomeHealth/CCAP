using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocument;

public sealed class GetPreAuthDocumentQueryHandler
    : IRequestHandler<GetPreAuthDocumentQuery, PreAuthDocumentDto?>
{
    private const string RequirementCode = "PRE_AUTH_RECEIVED";
    private readonly IPatientComplianceDocumentRepository _documents;

    public GetPreAuthDocumentQueryHandler(IPatientComplianceDocumentRepository documents)
    {
        _documents = documents;
    }

    public async Task<PreAuthDocumentDto?> Handle(
        GetPreAuthDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _documents.GetLatestAsync(
            request.PatientId,
            RequirementCode,
            cancellationToken);

        return document is null
            ? null
            : new PreAuthDocumentDto(
                document.PatientComplianceDocumentId,
                document.OriginalFileName,
                document.ContentType,
                document.FileSize,
                document.UploadedAt);
    }
}
