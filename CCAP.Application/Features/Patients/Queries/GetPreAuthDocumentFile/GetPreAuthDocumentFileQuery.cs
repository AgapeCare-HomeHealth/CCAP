using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocumentFile;

public sealed record GetPreAuthDocumentFileQuery(
    Guid PatientId) : IRequest<PreAuthDocumentFileDto?>;
