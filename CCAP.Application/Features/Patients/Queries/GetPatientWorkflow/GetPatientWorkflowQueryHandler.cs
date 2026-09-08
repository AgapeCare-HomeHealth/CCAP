using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Patients.DTOs;
using CCAP.Domain.Entities;
using CCAP.Domain.Enums;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatientWorkflow;

public sealed class GetPatientWorkflowQueryHandler
    : IRequestHandler<GetPatientWorkflowQuery, PatientWorkflowResponseDto?>
{
    private readonly IPatientRepository _patients;
    private readonly IUserRepository _users;

    public GetPatientWorkflowQueryHandler(
        IPatientRepository patients,
        IUserRepository users)
    {
        _patients = patients;
        _users = users;
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

        var socVisit = GetSocVisit(patient);

        const string insuranceVerificationRequirement =
            "INSURANCE_VERIFICATION";

        var insuranceVerification =
            patient.ComplianceRecords
                .Where(x =>
                    string.Equals(
                        x.RequirementCode,
                        insuranceVerificationRequirement,
                        StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.CompletedAt)
                .FirstOrDefault();

        ApplicationUser? insuranceVerifiedByUser = null;

        if (insuranceVerification?.IsCompleted == true &&
            insuranceVerification.CompletedByUserId.HasValue)
        {
            insuranceVerifiedByUser =
                await _users.GetByIdAsync(
                    insuranceVerification.CompletedByUserId.Value,
                    cancellationToken);
        }

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

        var completedByUserIds = patient.ComplianceRecords
            .Where(x => x.CompletedByUserId.HasValue)
            .Select(x => x.CompletedByUserId!.Value)
            .Distinct()
            .ToList();

        var completedByUsers =
            completedByUserIds.Count == 0
                ? []
                : await _users.GetByIdsAsync(
                    completedByUserIds,
                    cancellationToken);

        var completedByLookup =
            completedByUsers.ToDictionary(
                x => x.UserId,
                x => $"{x.FirstName} {x.LastName}".Trim());

        var complianceItems =
            patient.ComplianceRecords
                .OrderBy(x => GetComplianceOrder(x.RequirementCode))
                .Select(x => new ComplianceItemResponseDto
                {
                    ComplianceRecordId =
                        x.ComplianceRecordId,

                    RequirementCode =
                        x.RequirementCode,

                    RequirementName =
                        GetComplianceRequirementName(
                            x.RequirementCode),

                    Description =
                        x.Notes ?? string.Empty,

                    IsCompleted =
                        x.IsCompleted,

                    CompletedAt =
                        x.CompletedAt,

                    CompletedByUserId =
                        x.CompletedByUserId,

                    CompletedByUserName =
                        x.CompletedByUserId.HasValue &&
                        completedByLookup.TryGetValue(
                            x.CompletedByUserId.Value,
                            out var userName)
                            ? userName
                            : string.Empty
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

            WorkflowStages = BuildWorkflowStages(patient, referral),

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

                InsuranceMemberId =
                    patient.InsuranceMemberId ??
                    referral?.InsuranceMemberId ??
                    string.Empty,

                AuthorizationDate =
                    patient.AuthorizationDate ??
                    referral?.AuthorizationDate,

                AuthorizedVisits =
                    patient.ApprovedVisits ??
                    referral?.ApprovedVisits,

                AuthorizationRequired =
                    patient.AuthorizationRequired,

                SocDate =
                    patient.SocDate,

                SocVisitStatus =
                    socVisit is null
                        ? "Not Scheduled"
                        : string.Equals(
                            socVisit.Status,
                            "Completed",
                            StringComparison.OrdinalIgnoreCase)
                            ? "Completed"
                            : "Scheduled",

                SocScheduledAt =
                    socVisit?.ScheduledDate,

                SocCompletedAt =
                    socVisit?.CompletedDate,

                SocClinicianId =
                    socVisit?.ClinicianId,

                SocClinician =
                    socVisit?.Clinician is null
                        ? string.Empty
                        : $"{socVisit.Clinician.FirstName} {socVisit.Clinician.LastName}",

                Address =
                    BuildAddress(patient),

                PhoneNumber =
                    patient.PhoneNumber ?? string.Empty,

                // =========================================================
                // INSURANCE VERIFICATION
                // =========================================================

                InsuranceVerified =
                    insuranceVerification?.IsCompleted == true,

                InsuranceVerifiedAt =
                    insuranceVerification?.IsCompleted == true
                        ? insuranceVerification.CompletedAt
                        : null,

                InsuranceVerifiedByUserId =
                    insuranceVerification?.IsCompleted == true
                        ? insuranceVerification.CompletedByUserId
                        : null,

                InsuranceVerifiedBy =
                    insuranceVerifiedByUser is null
                        ? string.Empty
                        : $"{insuranceVerifiedByUser.FirstName} {insuranceVerifiedByUser.LastName}"
            },

            ComplianceItems = complianceItems
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
    Patient patient,
    Referral? referral)
    {
        var referralCompleted =
            referral is not null;

        var insuranceCompleted =
            patient.ComplianceRecords.Any(x =>
                string.Equals(
                    x.RequirementCode,
                    "INSURANCE_VERIFICATION",
                    StringComparison.OrdinalIgnoreCase)
                && x.IsCompleted);

        var physicianOrdersCompleted =
            patient.ComplianceRecords.Any(x =>
                string.Equals(
                    x.RequirementCode,
                    "PHYSICIAN_ORDERS",
                    StringComparison.OrdinalIgnoreCase)
                && x.IsCompleted);

        var socVisit = GetSocVisit(patient);

        var socScheduled =
            patient.SocDate.HasValue;


        var socVisitCompleted =
            socVisit is not null &&
            string.Equals(
                socVisit.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase);


        var admissionCompleted =
            socVisitCompleted;

        var hasVisits =
            patient.Visits.Any();

        var careCompleted =
            patient.CareCompletedAt.HasValue;

        var stages = new List<WorkflowStageResponseDto>
    {
        CreateWorkflowStage(
            patient,
            1,
            "REFERRAL",
            "Referral",
            referralCompleted,
            completedDate: referral?.ReferralDate),

        CreateWorkflowStage(
            patient,
            2,
            "INSURANCE",
            "Insurance",
            insuranceCompleted,
            completedDate:
                GetComplianceCompletedDate(
                    patient,
                    "INSURANCE_VERIFICATION")),

        CreateWorkflowStage(
            patient,
            3,
            "SOC",
            "SOC Scheduled",
            socScheduled,
            completedDate:
                socScheduled
                    ? patient.SocDate?.ToDateTime(
                        TimeOnly.MinValue)
                    : null),

        CreateWorkflowStage(
            patient,
            4,
            "ADMISSION",
            "Admission",
            admissionCompleted,
            completedDate:
                GetSocCompletionDate(patient)),

        CreateWorkflowStage(
            patient,
            5,
            "VISITS",
            "Visits",
            hasVisits),

        CreateWorkflowStage(
            patient,
            6,
            "RECERT",
            "Recertification",
            false),

        CreateWorkflowStage(
            patient,
            7,
            "DISCHARGE",
            "Discharge",
            careCompleted,
            completedDate:
                patient.CareCompletedAt)
    };

        SetWorkflowState(stages);

        return stages;
    }

    private static WorkflowStageResponseDto CreateWorkflowStage(
    Patient patient,
    int sequence,
    string stageCode,
    string stageName,
    bool completed,
    DateTime? completedDate = null)
    {
        return new WorkflowStageResponseDto
        {
            Sequence = sequence,

            StageCode = stageCode,

            StageName = stageName,

            Status = completed ? 2 : 0,

            Description = completed
                ? "Completed"
                : "Pending",

            CompletedDate = completedDate,

            AssignedUserId = patient.ClinicianId,

            AssignedUserName =
                patient.Clinician is null
                    ? string.Empty
                    : $"{patient.Clinician.FirstName} {patient.Clinician.LastName}",

            IsClickable = completed,

            Route =
                $"/tracker/patient/{patient.PatientId}"
        };
    }

    private static DateTime? GetComplianceCompletedDate(
    Patient patient,
    string requirementCode)
    {
        return patient.ComplianceRecords
            .Where(x =>
                string.Equals(
                    x.RequirementCode,
                    requirementCode,
                    StringComparison.OrdinalIgnoreCase)
                && x.IsCompleted)
            .OrderByDescending(x => x.CompletedAt)
            .Select(x => x.CompletedAt)
            .FirstOrDefault();
    }

    private static DateTime? GetSocCompletionDate(
    Patient patient)
    {
        var socVisit = GetSocVisit(patient);

        if (socVisit is null)
            return null;

        if (!string.Equals(
            socVisit.Status,
            "Completed",
            StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return socVisit.CompletedDate;
    }

    private static void SetWorkflowState(
    List<WorkflowStageResponseDto> stages)
    {
        var currentIndex =
            stages.FindIndex(
                x => x.Status != 2);

        if (currentIndex < 0)
            return;

        for (var i = 0; i < stages.Count; i++)
        {
            var stage = stages[i];

            if (i < currentIndex)
            {
                stage.Status = 2;
                stage.Description = "Completed";
                stage.IsClickable = true;
            }
            else if (i == currentIndex)
            {
                stage.Status = 1;
                stage.Description = "Current Stage";
                stage.IsClickable = true;
            }
            else
            {
                stage.Status = 0;
                stage.Description = "Pending";
                stage.IsClickable = false;
            }
        }
    }

    private static Visit? GetSocVisit(
    Patient patient)
    {
        if (!patient.SocDate.HasValue)
            return null;

        var socDate =
            patient.SocDate.Value
                .ToDateTime(TimeOnly.MinValue)
                .Date;

        return patient.Visits
            .Where(x =>
                x.ScheduledDate.Date == socDate)
            .OrderByDescending(x => x.ScheduledDate)
            .FirstOrDefault();
    }

    private static string GetComplianceRequirementName(
    string requirementCode)
    {
        return requirementCode
            .Trim()
            .ToUpperInvariant() switch
        {
            "REFERRAL_DOCUMENT" =>
                "Referral Document",

            "INSURANCE_VERIFICATION" =>
                "Insurance Verification",

            "PHYSICIAN_ORDERS" =>
                "Physician Orders",

            "SOC_SCHEDULING" =>
                "SOC Scheduling",

            "SOC_COMPLIANT" =>
                "SOC Compliant",

            "DME_MED_SUPPLY" =>
                "DME / Med Supply",

            "NOA_FILED" =>
                "NOA Filed",

            "OASIS_SOC_COMPLETE" =>
                "OASIS / SOC Complete",

            "QA_APPROVAL" =>
                "QA Approval – PO/POC Ready for Faxing",

            "ORDERS_SIGNED" =>
                "Orders Signed by Physician",

            "DOCS_UPLOADED" =>
                "Docs Uploaded",

            "SOC_FEEDBACK_QA" =>
                "SOC Feedback from QA",

            "CASE_MIX_IDENTIFIED" =>
                "Case Mix Identified",

            "CASE_MIX_ORDERS_SIGNED" =>
                "Case Mix Orders Signed",

            "CASE_MIX_PLOTTED" =>
                "Case Mix Plotted",

            _ =>
                requirementCode
        };
    }

    private static int GetComplianceOrder(
    string requirementCode)
    {
        return requirementCode
            .Trim()
            .ToUpperInvariant() switch
        {
            "REFERRAL_DOCUMENT" => 1,

            "INSURANCE_VERIFICATION" => 2,

            "PHYSICIAN_ORDERS" => 3,

            "SOC_SCHEDULING" => 4,

            "SOC_COMPLIANT" => 5,

            "DME_MED_SUPPLY" => 6,

            "NOA_FILED" => 7,

            "OASIS_SOC_COMPLETE" => 8,

            "QA_APPROVAL" => 9,

            "ORDERS_SIGNED" => 10,

            "DOCS_UPLOADED" => 11,

            "SOC_FEEDBACK_QA" => 12,

            "CASE_MIX_IDENTIFIED" => 13,

            "CASE_MIX_ORDERS_SIGNED" => 14,

            "CASE_MIX_PLOTTED" => 15,

            _ => 100
        };
    }

}