using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.UpdateWorkflowDetails;

public sealed class UpdateWorkflowDetailsCommandHandler : IRequestHandler<UpdateWorkflowDetailsCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientAuditLogRepository _auditLogs;
    private readonly IPatientTaskRepository _tasks;
    private readonly IComplianceRepository _compliance;

    public UpdateWorkflowDetailsCommandHandler(IPatientRepository patients, IUnitOfWork unitOfWork,
        IPatientAuditLogRepository auditLogs, IPatientTaskRepository tasks, IComplianceRepository compliance)
    { _patients = patients; _unitOfWork = unitOfWork; _auditLogs = auditLogs; _tasks = tasks; _compliance = compliance; }

    public async Task Handle(UpdateWorkflowDetailsCommand r, CancellationToken ct)
    {
        var p = await _patients.GetByIdForUpdateAsync(r.PatientId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");
        if (r.NumberOfVisits.HasValue && r.NumberOfVisits.Value <= 0)
            throw new InvalidOperationException("Number of Visits must be greater than zero.");

        var section = (r.Section ?? "All").Trim();
        bool Is(string value) => section.Equals(value, StringComparison.OrdinalIgnoreCase);
        bool all = Is("All");

        // Merge only the section being edited. Null/empty values from other sections
        // must never erase previously saved patient workflow data.
        p.UpdateWorkflowDetails(
            all || Is("Admission") ? r.PreAuthDueDate : p.PreAuthDueDate,
            all || Is("Admission") ? r.NumberOfVisits : p.NumberOfVisits,
            all || Is("OngoingCare") ? r.CaseMixType : p.CaseMixType,
            all || Is("Admission") ? r.HospitalDcSummaryNotes : p.HospitalDcSummaryNotes,
            all || Is("SOC") ? r.DmeMedSupplyNotes : p.DmeMedSupplyNotes,
            all || Is("SOC") ? r.SocFeedbackFromPatient : p.SocFeedbackFromPatient,
            all || Is("OngoingCare") ? r.TifDate : p.TifDate,
            all || Is("OngoingCare") ? r.RocDate : p.RocDate,
            all || Is("Recertification") ? r.RecertDate : p.RecertDate,
            all || Is("Recertification") ? r.PcpPtNotified : p.PcpPtNotified,
            all || Is("Discharge") ? r.DischargeDate : p.DischargeDate,
            all || Is("Discharge") ? r.DischargeFeedback : p.DischargeFeedback,
            all || Is("Discharge") ? r.TransferDestination : p.TransferDestination,
            all || Is("Discharge") ? r.TransferDate : p.TransferDate,
            all || Is("Discharge") ? r.TransferReason : p.TransferReason);

        // Dates, numbers, notes, and dropdowns saved in this section are informational
        // patient details. They do not create compliance records, tasks, or progression gates.
        // PCP/PT notification is the one explicit Yes/No checklist value also stored as a typed field.
        if (all || Is("Recertification"))
        {
            const string code = "PCP_PT_NOTIFIED";
            var record = p.ComplianceRecords.FirstOrDefault(x =>
                string.Equals(x.RequirementCode, code, StringComparison.OrdinalIgnoreCase));
            if (record is not null)
            {
                var desired = p.PcpPtNotified == true ? "Yes" : "No";
                if (!string.Equals(record.ComplianceStatus, desired, StringComparison.OrdinalIgnoreCase))
                    record.SetStatus(desired, r.UpdatedByUserId);
                await PatientComplianceTaskSynchronizer.EnsureAsync(p, code, _tasks, _compliance, ct);
            }
        }

        p.Activities.Add(new Activity(p.PatientId, r.UpdatedByUserId, "Workflow", "Workflow details updated", $"{section} workflow details updated."));
        await _auditLogs.AddAsync(new PatientAuditLog(p.PatientId, r.UpdatedByUserId, "PatientWorkflow", p.PatientId.ToString(), "UPDATE", null, "Workflow details updated", $"{section} workflow details updated."), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
