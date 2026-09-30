namespace CCAP.Application.Features.Patients.Queries.GetPatientAuditLog;
public sealed class PatientAuditLogResponseDto
{
 public Guid AuditLogId {get;set;} public DateTime OccurredAt {get;set;} public string EntityType {get;set;}=""; public string EntityId {get;set;}=""; public string Action {get;set;}=""; public string? OldValues {get;set;} public string? NewValues {get;set;} public string? Description {get;set;} public string PerformedBy {get;set;}="System";
}
