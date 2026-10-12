using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UnmarkCompliance;
public sealed class UnmarkComplianceCommandHandler : IRequestHandler<UnmarkComplianceCommand>
{
 private readonly IPatientRepository _patients; private readonly IComplianceRepository _compliance; private readonly IUnitOfWork _unit; private readonly IPatientAuditLogRepository _audit;
 public UnmarkComplianceCommandHandler(IPatientRepository patients,IComplianceRepository compliance,IUnitOfWork unit,IPatientAuditLogRepository audit){_patients=patients;_compliance=compliance;_unit=unit;_audit=audit;}
 public async Task Handle(UnmarkComplianceCommand r,CancellationToken ct){
  var patient=await _patients.GetByIdForUpdateAsync(r.PatientId,ct) ?? throw new KeyNotFoundException("Patient not found.");
  var code=r.RequirementCode.Trim().ToUpperInvariant(); var c=await _compliance.GetByPatientAndRequirementAsync(r.PatientId,code,ct) ?? throw new KeyNotFoundException($"Compliance requirement '{code}' was not found.");
  if(!c.IsCompleted)return; c.RequireReverification();
  if(string.Equals(code,"INSURANCE_VERIFICATION",StringComparison.OrdinalIgnoreCase)) patient.ClearInsuranceVerification();
  patient.Activities.Add(new CCAP.Domain.Entities.Activity(patient.PatientId,r.UserId,"Compliance","Compliance unmarked",$"{code} returned to pending for reverification."));
  await _audit.AddAsync(new CCAP.Domain.Entities.PatientAuditLog(patient.PatientId,r.UserId,"Compliance",c.ComplianceRecordId.ToString(),"UPDATE","Completed","Pending",$"{code} unmarked for reverification."),ct);
  await _unit.SaveChangesAsync(ct);
 }
}
