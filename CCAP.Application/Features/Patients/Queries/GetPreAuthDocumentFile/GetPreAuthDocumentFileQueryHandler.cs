using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocumentFile;

public sealed class GetPreAuthDocumentFileQueryHandler
    : IRequestHandler<GetPreAuthDocumentFileQuery, PreAuthDocumentFileDto?>
{
    private const string RequirementCode = "PRE_AUTH_RECEIVED";
    private readonly IPatientComplianceDocumentRepository _documents;

    public GetPreAuthDocumentFileQueryHandler(IPatientComplianceDocumentRepository documents)
    {
        _documents = documents;
    }

    public async Task<PreAuthDocumentFileDto?> Handle(
        GetPreAuthDocumentFileQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _documents.GetLatestAsync(
            request.PatientId,
            RequirementCode,
            cancellationToken);

        if (document is null || string.IsNullOrWhiteSpace(document.StorageKey))
            return null;

        return new PreAuthDocumentFileDto(
            document.StorageKey,
            document.OriginalFileName,
            document.ContentType);
    }
}
