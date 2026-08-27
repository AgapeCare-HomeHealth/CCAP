using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Patients.DTOs;
using CCAP.Domain.Enums;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatientWorkflow;

public sealed class GetPatientWorkflowQueryHandler
    : IRequestHandler<GetPatientWorkflowQuery, PatientWorkflowResponseDto?>
{
    private readonly IPatientRepository _patients;

    public GetPatientWorkflowQueryHandler(
        IPatientRepository patients)
    {
        _patients = patients;
    }

    public async Task<PatientWorkflowResponseDto?> Handle(
        GetPatientWorkflowQuery request,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(
            request.PatientId,
            cancellationToken);

        if (patient is null)
            return null;

        var referral = patient.Referrals
            .OrderByDescending(x => x.ReferralDate)
            .FirstOrDefault();

        var coordinator = patient.Coordinator is null
            ? string.Empty
            : $"{patient.Coordinator.FirstName} {patient.Coordinator.LastName}";

        var clinician = patient.Clinician is null
            ? string.Empty
            : $"{patient.Clinician.FirstName} {patient.Clinician.LastName}";

        var age = CalculateAge(patient.DateOfBirth);

        var pendingTask = patient.Tasks
            .Where(x =>
                x.Status != PatientTaskStatus.Completed &&
                x.Status != PatientTaskStatus.Cancelled)
            .OrderBy(x => x.DueDate)
            .FirstOrDefault();

        var activities = patient.Activities
            .OrderByDescending(x => x.ActivityDate)
            .Take(3)
            .Select(x => new ActivityResponseDto
            {
                ActivityId = x.ActivityId,
                ActivityDate = x.ActivityDate,
                Title = x.Title,
                Description = x.Description,
                PerformedBy = x.PerformedBy is null
                    ? "System"
                    : $"{x.PerformedBy.FirstName} {x.PerformedBy.LastName}",
                ActivityType = x.ActivityType
            })
            .ToList();

        return new PatientWorkflowResponseDto
        {
            Header = new PatientHeaderResponseDto
            {
                PatientId = patient.PatientId,

                ReferralId = referral?.ReferralId ?? Guid.Empty,

                FirstName = patient.FirstName,

                MiddleName = patient.MiddleName,

                LastName = patient.LastName,

                Age = age,

                MRN = patient.MRN,

                ReferralNumber =
                    referral?.ReferralNumber ?? string.Empty,

                Status = patient.Status.ToString(),

                SocDate = patient.SocDate,

                Coordinator = coordinator,

                Branch = referral?.Location?.Name ?? string.Empty,

                EpisodeNumber = 1
            },

            WorkflowStages = BuildWorkflowStages(patient),

            NextAction = pendingTask is null
                ? new NextActionResponseDto
                {
                    TaskId = Guid.Empty,
                    Title = "No pending actions",
                    Description = "There are currently no pending patient tasks.",
                    DueDate = DateTime.Now,
                    PageRoute = string.Empty,
                    Icon = "bi bi-check2-circle",
                    IsOverdue = false
                }
                : new NextActionResponseDto
                {
                    TaskId = pendingTask.TaskId,
                    Title = pendingTask.Title,
                    Description = pendingTask.Description,
                    DueDate = pendingTask.DueDate,
                    PageRoute = pendingTask.PageRoute ?? string.Empty,
                    Icon = "bi bi-check2-circle",
                    IsOverdue = pendingTask.DueDate < DateTime.Now
                },

            KeyInformation = new KeyInformationResponseDto
            {
                Coordinator = coordinator,

                Clinician = clinician,

                Discipline =
                    referral?.Discipline?.Name ?? string.Empty,

                Episode = 1,

                Branch =
                    referral?.Location?.Name ?? string.Empty,

                Payor =
                    patient.PrimaryInsurance ??
                    referral?.PrimaryInsurance ??
                    string.Empty,

                Priority =
                    referral?.Priority ?? "Routine"
            },

            RecentActivities = activities,

            Summary = new PatientSummaryResponseDto
            {
                PrimaryDiagnosis =
                    patient.PrimaryDiagnosis ?? string.Empty,

                Insurance =
                    patient.PrimaryInsurance ??
                    referral?.PrimaryInsurance ??
                    string.Empty,

                SocDate = patient.SocDate,

                // There is currently no AuthorizedVisits
                // property in the Patient entity.
                AuthorizedVisits = 0,

                Address = BuildAddress(patient),

                PhoneNumber =
                    patient.PhoneNumber ?? string.Empty
            }
        };
    }

    private static int CalculateAge(DateOnly? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return 0;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var age = today.Year - dateOfBirth.Value.Year;

        if (dateOfBirth.Value > today.AddYears(-age))
            age--;

        return age;
    }

    private static string BuildAddress(
        CCAP.Domain.Entities.Patient patient)
    {
        return string.Join(
            ", ",
            new[]
            {
                patient.Address,
                patient.City,
                patient.State,
                patient.ZipCode
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static List<WorkflowStageResponseDto> BuildWorkflowStages(
        CCAP.Domain.Entities.Patient patient)
    {
        var currentStage = patient.Status switch
        {
            PatientStatus.OnHold => 2,
            PatientStatus.Completed => 6,
            PatientStatus.Archived => 7,
            PatientStatus.Cancelled => 7,
            _ => 3
        };

        var stages = new[]
        {
            ("REFERRAL", "Referral"),
            ("INSURANCE", "Insurance"),
            ("SOC", "SOC Scheduled"),
            ("ADMISSION", "Admission"),
            ("VISITS", "Visits"),
            ("RECERT", "Recertification"),
            ("DISCHARGE", "Discharge")
        };

        return stages
            .Select((stage, index) =>
            {
                var sequence = index + 1;

                var status =
                    sequence < currentStage
                        ? 2       // Completed
                        : sequence == currentStage
                            ? 1   // Current
                            : 0;  // Pending

                return new WorkflowStageResponseDto
                {
                    Sequence = sequence,

                    StageCode = stage.Item1,

                    StageName = stage.Item2,

                    Status = status,

                    Description =
                        status == 2
                            ? "Completed"
                            : status == 1
                                ? "Current Stage"
                                : "Pending",

                    CompletedDate =
                        status == 2
                            ? DateTime.Today
                            : null,

                    AssignedUserId = patient.ClinicianId,

                    AssignedUserName =
                        patient.Clinician is null
                            ? string.Empty
                            : $"{patient.Clinician.FirstName} {patient.Clinician.LastName}",

                    IsClickable = sequence <= currentStage,

                    Route =
                        $"/tracker/patient/{patient.PatientId}"
                };
            })
            .ToList();
    }
}