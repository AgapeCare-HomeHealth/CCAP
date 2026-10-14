using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Patients.Commands.SetComplianceStatus;
public sealed class SetComplianceStatusCommandHandler : IRequestHandler<SetComplianceStatusCommand>
{
    private readonly IPatientRepository _patients; private readonly IComplianceRepository _compliance; private readonly IUnitOfWork _unit; private readonly IPatientAuditLogRepository _audit; private readonly IPatientTaskRepository _tasks;
    public SetComplianceStatusCommandHandler(IPatientRepository patients, IComplianceRepository compliance, IUnitOfWork unit, IPatientAuditLogRepository audit, IPatientTaskRepository tasks) { _patients=patients; _compliance=compliance; _unit=unit; _audit=audit; _tasks=tasks; }
    public async Task Handle(SetComplianceStatusCommand r, CancellationToken ct)
    {
        var status=r.Status?.Trim(); if (status is null || !(status.Equals("Yes",StringComparison.OrdinalIgnoreCase) || status.Equals("No",StringComparison.OrdinalIgnoreCase) || status.Equals("N/A",StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("Status must be Yes, No, or N/A.");
        var patient=await _patients.GetByIdForUpdateAsync(r.PatientId,ct) ?? throw new KeyNotFoundException("Patient not found.");
        var code=r.RequirementCode.Trim().ToUpperInvariant(); var c=await _compliance.GetByPatientAndRequirementAsync(r.PatientId,code,ct) ?? throw new KeyNotFoundException($"Compliance requirement '{code}' was not found.");
        var old=c.ComplianceStatus ?? (c.IsCompleted ? "Yes" : "No"); if (old.Equals(status,StringComparison.OrdinalIgnoreCase)) return;
        c.SetStatus(status,r.UserId);
        if (code=="INSURANCE_VERIFICATION") { if (c.IsCompleted && !patient.InsuranceVerifiedAt.HasValue) patient.VerifyInsurance(r.UserId); else if (!c.IsCompleted) patient.ClearInsuranceVerification(); }
        var newStatus=status.Equals("N/A",StringComparison.OrdinalIgnoreCase)?"N/A":status.Equals("Yes",StringComparison.OrdinalIgnoreCase)?"Completed":"Pending";
        patient.Activities.Add(new CCAP.Domain.Entities.Activity(patient.PatientId,r.UserId,"Compliance","Compliance status changed",$"{code}: {old} → {status}."));
        await _audit.AddAsync(new CCAP.Domain.Entities.PatientAuditLog(patient.PatientId,r.UserId,"Compliance",c.ComplianceRecordId.ToString(),"UPDATE",old,newStatus,$"{code} compliance status changed from {old} to {status}."),ct);
        if (c.IsCompleted) { var title=code switch { "INSURANCE_VERIFICATION" => "Verify Insurance", "PHYSICIAN_ORDERS" or "ORDERS_SIGNED" => "Verify Physician Orders", _ => null }; if(title is not null) { var task=await _tasks.GetPendingByPatientAndTitleAsync(r.PatientId,title,ct); task?.Complete(); } }
        await _unit.SaveChangesAsync(ct);
    }
}
