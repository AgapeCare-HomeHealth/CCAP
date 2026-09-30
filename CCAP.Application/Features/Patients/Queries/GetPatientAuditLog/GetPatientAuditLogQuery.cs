using MediatR;
namespace CCAP.Application.Features.Patients.Queries.GetPatientAuditLog;
public sealed record GetPatientAuditLogQuery(Guid PatientId) : IRequest<List<PatientAuditLogResponseDto>>;
