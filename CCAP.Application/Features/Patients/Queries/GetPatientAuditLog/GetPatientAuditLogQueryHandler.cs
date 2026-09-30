using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Patients.Queries.GetPatientAuditLog;
public sealed class GetPatientAuditLogQueryHandler : IRequestHandler<GetPatientAuditLogQuery,List<PatientAuditLogResponseDto>>
{
 private readonly IPatientAuditLogRepository _repo; public GetPatientAuditLogQueryHandler(IPatientAuditLogRepository repo)=>_repo=repo;
 public async Task<List<PatientAuditLogResponseDto>> Handle(GetPatientAuditLogQuery r,CancellationToken ct){var logs=await _repo.GetByPatientIdAsync(r.PatientId,ct); return logs.Select(x=>new PatientAuditLogResponseDto{AuditLogId=x.PatientAuditLogId,OccurredAt=x.OccurredAt,EntityType=x.EntityType,EntityId=x.EntityId,Action=x.Action,OldValues=x.OldValues,NewValues=x.NewValues,Description=x.Description,PerformedBy=x.PerformedByUser is null?"System":$"{x.PerformedByUser.FirstName} {x.PerformedByUser.LastName}".Trim()}).ToList();}
}
