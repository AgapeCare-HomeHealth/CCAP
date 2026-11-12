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

        if (all || Is("Recertification"))
        {
            if (r.RecertificationRecords is not null)
            {
                foreach (var item in r.RecertificationRecords)
                {
                    if (item.PcpPtNotified is not null &&
                        item.PcpPtNotified is not ("YES" or "NO" or "N/A"))
                        throw new InvalidOperationException("PCP & PT Notified must be YES, NO, or N/A.");
                }

                p.SetRecertificationRecordsJson(System.Text.Json.JsonSerializer.Serialize(
                    r.RecertificationRecords.Select(x => new
                    {
                        x.Id,
                        x.ReceivedDate,
                        PcpPtNotified = string.IsNullOrWhiteSpace(x.PcpPtNotified) ? null : x.PcpPtNotified.Trim().ToUpperInvariant()
                    })));
            }
        }

        // Dates, numbers, notes, and dropdowns saved in this section are informational
        // patient details. They do not create compliance records, tasks, or progression gates.
        p.Activities.Add(new Activity(p.PatientId, r.UpdatedByUserId, "Workflow", "Workflow details updated", $"{section} workflow details updated."));
        await _auditLogs.AddAsync(new PatientAuditLog(p.PatientId, r.UpdatedByUserId, "PatientWorkflow", p.PatientId.ToString(), "UPDATE", null, "Workflow details updated", $"{section} workflow details updated."), ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
