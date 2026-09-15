using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.ScheduleSoc;

public sealed class ScheduleSocCommandHandler
    : IRequestHandler<ScheduleSocCommand>
{
    private const string SocSchedulingRequirement =
        "SOC_SCHEDULING";

    private readonly IPatientRepository _patients;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientAuditLogRepository _auditLogs;

    public ScheduleSocCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork, IPatientAuditLogRepository auditLogs)
    {
        _patients = patients;

        _unitOfWork = unitOfWork;
        _auditLogs = auditLogs;
    }


    public async Task Handle(
        ScheduleSocCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await _patients.GetByIdForWorkflowUpdateAsync(
                request.PatientId,
                cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }


        // Insurance verification is a Patient state, not an Excel
        // compliance checklist item. The persisted source of truth is
        // InsuranceVerifiedAt.
        var insuranceVerified = patient.InsuranceVerifiedAt.HasValue;

        if (!insuranceVerified)
        {
            throw new InvalidOperationException(
                "Insurance must be verified before scheduling SOC.");
        }


        if (!patient.ClinicianId.HasValue)
        {
            throw new InvalidOperationException(
                "A clinician must be assigned before scheduling SOC.");
        }


        var scheduledDate =
            request.SocDate.ToDateTime(
                TimeOnly.MinValue);


        var existingSocVisit =
            patient.Visits
                .Where(x =>
                    string.Equals(
                        x.Status,
                        "Scheduled",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    patient.SocDate.HasValue
                    &&
                    x.ScheduledDate.Date ==
                    patient.SocDate.Value
                        .ToDateTime(TimeOnly.MinValue)
                        .Date)
                .OrderByDescending(x => x.ScheduledDate)
                .FirstOrDefault();


        patient.SetSocDate(
            request.SocDate);


        if (existingSocVisit is null)
        {
            patient.Visits.Add(
                new Visit(
                    patient.PatientId,
                    patient.ClinicianId.Value,
                    scheduledDate));
        }
        else
        {
            existingSocVisit.Reschedule(
                scheduledDate);
        }


        var schedulingRequirement =
            patient.ComplianceRecords
                .FirstOrDefault(x =>
                    string.Equals(
                        x.RequirementCode,
                        SocSchedulingRequirement,
                        StringComparison.OrdinalIgnoreCase));


        if (schedulingRequirement is null)
        {
            schedulingRequirement =
                new ComplianceRecord(
                    patient.PatientId,
                    SocSchedulingRequirement,
                    "Start of Care visit must be scheduled.");

            patient.ComplianceRecords.Add(
                schedulingRequirement);
        }


        if (!schedulingRequirement.IsCompleted)
        {
            schedulingRequirement.Complete(
                request.ScheduledByUserId);
        }


        foreach (var task in patient.Tasks)
        {
            if (task.Status == Domain.Enums.PatientTaskStatus.Completed ||
                task.Status == Domain.Enums.PatientTaskStatus.Cancelled)
                continue;

            if (task.Title.Contains("Schedule SOC", StringComparison.OrdinalIgnoreCase))
                task.Complete();
        }

        var hasPendingSocTask = patient.Tasks.Any(x =>
            x.Status != Domain.Enums.PatientTaskStatus.Completed &&
            x.Status != Domain.Enums.PatientTaskStatus.Cancelled &&
            x.Title.Contains("Complete SOC", StringComparison.OrdinalIgnoreCase));

        if (!hasPendingSocTask)
        {
            var completeSocTask = new PatientTask(
                patient.PatientId,
                "Complete SOC",
                "Complete the scheduled Start of Care visit.",
                scheduledDate,
                $"/tracker/patient/{patient.PatientId}");

            completeSocTask.Assign(patient.ClinicianId.Value);
            patient.Tasks.Add(completeSocTask);
        }

        patient.Activities.Add(new CCAP.Domain.Entities.Activity(patient.PatientId, request.ScheduledByUserId, "Workflow", "SOC scheduled", $"SOC scheduled for {request.SocDate:MM/dd/yyyy}."));
        await _auditLogs.AddAsync(new PatientAuditLog(patient.PatientId, request.ScheduledByUserId, "Patient", patient.PatientId.ToString(), "UPDATE", null, System.Text.Json.JsonSerializer.Serialize(new { SocDate = request.SocDate, Action = "SOC scheduled" }), "SOC date/visit scheduled."), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}