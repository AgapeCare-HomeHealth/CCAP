using CCAP.Application.Features.Patients;
using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UnmarkCompliance;
public sealed class UnmarkComplianceCommandHandler : IRequestHandler<UnmarkComplianceCommand>
{
 private readonly IPatientRepository _patients; private readonly IComplianceRepository _compliance; private readonly IUnitOfWork _unit; private readonly IPatientAuditLogRepository _audit; private readonly IPatientTaskRepository _tasks;
 public UnmarkComplianceCommandHandler(IPatientRepository patients,IComplianceRepository compliance,IUnitOfWork unit,IPatientAuditLogRepository audit,IPatientTaskRepository tasks){_patients=patients;_compliance=compliance;_unit=unit;_audit=audit;_tasks=tasks;}
 public async Task Handle(UnmarkComplianceCommand r,CancellationToken ct){
  var patient=await _patients.GetByIdForUpdateAsync(r.PatientId,ct) ?? throw new KeyNotFoundException("Patient not found.");
  var code=r.RequirementCode.Trim().ToUpperInvariant(); var c=await _compliance.GetByPatientAndRequirementAsync(r.PatientId,code,ct) ?? throw new KeyNotFoundException($"Compliance requirement '{code}' was not found.");
  if(!c.IsCompleted)return; c.RequireReverification();
  if(string.Equals(code,"INSURANCE_VERIFICATION",StringComparison.OrdinalIgnoreCase)) patient.ClearInsuranceVerification();
  await PatientComplianceTaskSynchronizer.EnsureAsync(patient, code, _tasks, _compliance, ct);
        await _unit.SaveChangesAsync(ct);
 }
}
