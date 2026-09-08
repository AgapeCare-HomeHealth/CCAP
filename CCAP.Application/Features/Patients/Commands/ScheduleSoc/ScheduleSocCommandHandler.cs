using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.ScheduleSoc;

public sealed class ScheduleSocCommandHandler
    : IRequestHandler<ScheduleSocCommand>
{
    private const string InsuranceVerificationRequirement =
        "INSURANCE_VERIFICATION";

    private const string SocSchedulingRequirement =
        "SOC_SCHEDULING";

    private readonly IPatientRepository _patients;

    private readonly IUnitOfWork _unitOfWork;

    public ScheduleSocCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;

        _unitOfWork = unitOfWork;
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


        var insuranceVerified =
            patient.ComplianceRecords.Any(x =>
                string.Equals(
                    x.RequirementCode,
                    InsuranceVerificationRequirement,
                    StringComparison.OrdinalIgnoreCase)
                && x.IsCompleted);

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
            if (task.Status ==
                    Domain.Enums.PatientTaskStatus.Completed
                ||
                task.Status ==
                    Domain.Enums.PatientTaskStatus.Cancelled)
            {
                continue;
            }

            if (task.Title.Contains(
                    "Schedule SOC",
                    StringComparison.OrdinalIgnoreCase))
            {
                task.Complete();
            }
        }


        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}