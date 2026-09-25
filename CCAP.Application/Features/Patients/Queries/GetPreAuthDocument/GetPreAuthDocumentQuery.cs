using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocument;

public sealed record GetPreAuthDocumentQuery(
    Guid PatientId) : IRequest<PreAuthDocumentDto?>;
