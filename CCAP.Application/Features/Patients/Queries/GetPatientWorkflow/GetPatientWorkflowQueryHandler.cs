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
    private readonly IPatientCareLogRepository _careLogs;

    public GetPatientWorkflowQueryHandler(
        IPatientRepository patients,
        IUserRepository users,
        IPatientCareLogRepository careLogs)
    {
        _patients = patients;
        _users = users;
        _careLogs = careLogs;
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

        ApplicationUser? insuranceVerifiedByUser = null;

        if (patient.InsuranceVerifiedByUserId.HasValue)
        {
            insuranceVerifiedByUser =
                await _users.GetByIdAsync(
                    patient.InsuranceVerifiedByUserId.Value,
                    cancellationToken);
        }

        var coordinator = patient.Coordinator is null
            ? string.Empty
            : $"{patient.Coordinator.FirstName} {patient.Coordinator.LastName}";

        var clinician = patient.Clinician is null
            ? string.Empty
            : $"{patient.Clinician.FirstName} {patient.Clinician.LastName}";

        var age = CalculateAge(patient.DateOfBirth);

        var activeComplianceRequirementCodes =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "PRE_AUTH_RECEIVED",
                "PRO_RECEIVED",
                "H_AND_P_RECEIVED",
                "F2F_RECEIVED",
                "PCP_CONFIRMED",
                "AVS_RECEIVED",
                "SIGNED_CONSENTS",
                "INSURANCE_VERIFICATION",

                "SOC_COMPLIANT",
                "DME_MED_SUPPLY",
                "NOA_FILED",
                "OASIS_SOC_COMPLETE",
                "QA_APPROVAL",
                "ORDERS_SIGNED",
                "DOCS_UPLOADED",
                                "CASE_MIX_IDENTIFIED",
                "CASE_MIX_ORDERS_SIGNED",
                "CASE_MIX_PLOTTED",
                "PCP_PT_NOTIFIED",

                "ROC",
                "RECERTIFICATION",
                "DISCHARGE_SUMMARY_SIGNED",
                "NOMNC_SIGNED"
            };

        var pendingTask = patient.Tasks
            .Where(x => x.Status != PatientTaskStatus.Completed && x.Status != PatientTaskStatus.Cancelled)
            .OrderBy(x => x.DueDate)
            .FirstOrDefault();

        var nextCompliance = patient.ComplianceRecords
            .Where(x => !x.IsCompleted && activeComplianceRequirementCodes.Contains(x.RequirementCode) && GetCompliancePhase(x.RequirementCode) <= 2)
            .OrderBy(x => GetComplianceOrder(x.RequirementCode))
            .FirstOrDefault();

        var activities = patient.Activities
            .OrderByDescending(x => x.ActivityDate)
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

        var careLogs = await _careLogs.GetByPatientIdAsync(request.PatientId, cancellationToken);

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



        // Timeline is a patient-history view, not an audit log. It combines
        // actual workflow/history records from the patient's existing entities.
        var timelineEvents = new List<PatientTimelineEventResponseDto>();

        timelineEvents.AddRange(patient.Activities.Select(x => new PatientTimelineEventResponseDto
        {
            EventId = x.ActivityId,
            EventDate = x.ActivityDate,
            EventType = string.IsNullOrWhiteSpace(x.ActivityType) ? "Activity" : x.ActivityType,
            Title = x.Title,
            Description = x.Description,
            PerformedBy = x.PerformedBy is null ? "System" : $"{x.PerformedBy.FirstName} {x.PerformedBy.LastName}"
        }));

        foreach (var referralItem in patient.Referrals)
        {
            timelineEvents.Add(new PatientTimelineEventResponseDto
            {
                EventId = referralItem.ReferralId,
                EventDate = referralItem.ReferralDate,
                EventType = "Referral",
                Title = "Referral received",
                Description = string.IsNullOrWhiteSpace(referralItem.ReferralNotes) ? "Referral record created." : referralItem.ReferralNotes,
                PerformedBy = "System"
            });

            if (referralItem.AssignedAt.HasValue)
            {
                timelineEvents.Add(new PatientTimelineEventResponseDto
                {
                    EventId = Guid.NewGuid(),
                    EventDate = referralItem.AssignedAt.Value,
                    EventType = "Referral",
                    Title = "Referral assigned",
                    Description = string.IsNullOrWhiteSpace(referralItem.InternalNotes) ? "Referral assigned for processing." : referralItem.InternalNotes,
                    PerformedBy = referralItem.AssignedUser is null ? "System" : $"{referralItem.AssignedUser.FirstName} {referralItem.AssignedUser.LastName}"
                });
            }
        }

        foreach (var note in patient.CallNotes)
        {
            timelineEvents.Add(new PatientTimelineEventResponseDto
            {
                EventId = note.CallNoteId,
                EventDate = note.CallDate,
                EventType = "Communication",
                Title = string.IsNullOrWhiteSpace(note.Subject) ? "Communication note" : note.Subject,
                Description = note.Notes,
                PerformedBy = note.RecordedBy is null ? "System" : $"{note.RecordedBy.FirstName} {note.RecordedBy.LastName}"
            });
        }

        foreach (var visit in patient.Visits)
        {
            timelineEvents.Add(new PatientTimelineEventResponseDto
            {
                EventId = visit.VisitId,
                EventDate = visit.CompletedDate ?? visit.ScheduledDate,
                EventType = "Visit",
                Title = $"{visit.VisitType} - {visit.Status}",
                Description = string.Join(" | ", new[] { visit.CallNotes, visit.NotesFlag, visit.Notes }.Where(v => !string.IsNullOrWhiteSpace(v))),
                PerformedBy = visit.Clinician is null ? (visit.ClinicianName ?? "System") : $"{visit.Clinician.FirstName} {visit.Clinician.LastName}"
            });
        }

        foreach (var assessment in patient.Assessments.Where(x => x.CompletedAt.HasValue))
        {
            timelineEvents.Add(new PatientTimelineEventResponseDto
            {
                EventId = assessment.AssessmentId,
                EventDate = assessment.CompletedAt!.Value,
                EventType = "Assessment",
                Title = "Assessment completed",
                Description = assessment.Notes ?? string.Empty,
                PerformedBy = assessment.CompletedBy is null ? "System" : $"{assessment.CompletedBy.FirstName} {assessment.CompletedBy.LastName}"
            });
        }

        foreach (var complianceItem in patient.ComplianceRecords.Where(x => x.IsCompleted && x.CompletedAt.HasValue))
        {
            timelineEvents.Add(new PatientTimelineEventResponseDto
            {
                EventId = complianceItem.ComplianceRecordId,
                EventDate = complianceItem.CompletedAt!.Value,
                EventType = "Compliance",
                Title = $"Compliance completed: {GetComplianceRequirementName(complianceItem.RequirementCode)}",
                Description = complianceItem.Notes ?? string.Empty,
                PerformedBy = complianceItem.CompletedByUserId.HasValue && completedByLookup.TryGetValue(complianceItem.CompletedByUserId.Value, out var complianceUser) ? complianceUser : "System"
            });
        }

        foreach (var log in careLogs)
        {
            timelineEvents.Add(new PatientTimelineEventResponseDto
            {
                EventId = log.PatientCareLogId,
                EventDate = log.RecordedAt,
                EventType = "Care",
                Title = string.IsNullOrWhiteSpace(log.Item) ? "Care log recorded" : log.Item,
                Description = string.Join(" | ", new[] { log.LogType, log.Notes }.Where(v => !string.IsNullOrWhiteSpace(v))),
                PerformedBy = log.RecordedByUser is null ? "System" : $"{log.RecordedByUser.FirstName} {log.RecordedByUser.LastName}"
            });
        }

        foreach (var task in patient.Tasks.Where(x => x.CompletedAt.HasValue))
        {
            timelineEvents.Add(new PatientTimelineEventResponseDto
            {
                EventId = task.TaskId,
                EventDate = task.CompletedAt!.Value,
                EventType = "Task",
                Title = $"Task completed: {task.Title}",
                Description = task.Description,
                PerformedBy = task.AssignedUser is null ? "System" : $"{task.AssignedUser.FirstName} {task.AssignedUser.LastName}"
            });
        }

        timelineEvents = timelineEvents
            .Where(x => x.EventDate != default)
            .OrderByDescending(x => x.EventDate)
            .ToList();

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
                            : string.Empty,

                    Phase =
                        GetCompliancePhase(
                            x.RequirementCode)
                })
                .ToList();

        return new PatientWorkflowResponseDto
        {
            ReferralDate = referral?.ReferralDate,
            ReferralNotes = referral?.ReferralNotes ?? string.Empty,
            InternalNotes = referral?.InternalNotes ?? string.Empty,

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

            NextAction = pendingTask is not null
                ? new NextActionResponseDto
                {
                    TaskId = pendingTask.TaskId, Title = pendingTask.Title, Description = pendingTask.Description,
                    DueDate = pendingTask.DueDate, PageRoute = string.IsNullOrWhiteSpace(pendingTask.PageRoute) ? $"/tracker/patient/{patient.PatientId}" : pendingTask.PageRoute, Icon = "bi bi-check2-circle", IsOverdue = pendingTask.DueDate < DateTime.Now
                }
                : nextCompliance is not null
                    ? new NextActionResponseDto
                    {
                        TaskId = Guid.Empty, Title = $"Confirm {GetComplianceRequirementName(nextCompliance.RequirementCode)}",
                        Description = "Check the corresponding information in the EMR, then mark the CCAP requirement complete.",
                        DueDate = DateTime.Now, PageRoute = $"/tracker/patient/{patient.PatientId}", Icon = "bi bi-clipboard-check", IsOverdue = false
                    }
                    : new NextActionResponseDto
                    { TaskId = Guid.Empty, Title = "No pending actions", Description = "There are currently no pending workflow actions.", DueDate = DateTime.Now, PageRoute = string.Empty, Icon = "bi bi-check2-circle", IsOverdue = false },

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
                    referral?.Priority ?? "Routine",

                PrimaryCarePhysician = referral?.ReferringPhysician ?? string.Empty,
                PhysicianFax = referral?.PhysicianFax ?? string.Empty,
                PhysicianNpi = referral?.PhysicianNpi ?? string.Empty,
                PcosRegistered = referral?.PcosRegistered ?? string.Empty
            },

            RecentActivities = activities,
            TimelineEvents = timelineEvents,

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

                SocVisitStatus = "Managed in EMR",

                SocScheduledAt = null,

                SocCompletedAt = null,

                SocClinicianId = null,

                SocClinician = string.Empty,

                Address =
                    BuildAddress(patient),

                PhoneNumber =
                    patient.PhoneNumber ?? string.Empty,

                // =========================================================
                // INSURANCE VERIFICATION
                // =========================================================

                InsuranceVerified =
                    patient.InsuranceVerifiedAt.HasValue,

                InsuranceVerifiedAt =
                    patient.InsuranceVerifiedAt,

                InsuranceVerifiedByUserId =
                    patient.InsuranceVerifiedByUserId,

                InsuranceVerifiedBy =
                    insuranceVerifiedByUser is null
                        ? string.Empty
                        : $"{insuranceVerifiedByUser.FirstName} {insuranceVerifiedByUser.LastName}"
            },

            ComplianceItems = complianceItems,

            WorkflowDetails = new WorkflowDetailsResponseDto
            {
                PreAuthDueDate = patient.PreAuthDueDate,
                NumberOfVisits = patient.NumberOfVisits,
                CaseMixType = patient.CaseMixType ?? string.Empty,
                DmeMedSupplyNotes = patient.DmeMedSupplyNotes ?? string.Empty,
                SocFeedbackFromPatient = patient.SocFeedbackFromPatient ?? string.Empty,
                TifDate = patient.TifDate,
                RocDate = patient.RocDate,
                RecertDate = patient.RecertDate,
                PcpPtNotified = patient.PcpPtNotified,
                DischargeDate = patient.DischargeDate ?? (patient.CareCompletedAt.HasValue ? DateOnly.FromDateTime(patient.CareCompletedAt.Value) : null),
                DischargeFeedback = patient.DischargeFeedback ?? string.Empty,
                TransferDestination = patient.TransferDestination ?? string.Empty,
                TransferDate = patient.TransferDate,
                TransferReason = patient.TransferReason ?? string.Empty
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

        return Math.Max(age, 0);
    }

    private static string BuildAddress(Patient patient)
    {
        var parts = new[]
        {
            patient.Address,
            patient.City,
            patient.State,
            patient.ZipCode
        }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => x!.Trim())
        .ToArray();

        return string.Join(", ", parts);
    }

    private static int GetCompliancePhase(string requirementCode)
    {
        return requirementCode.Trim().ToUpperInvariant() switch
        {
            "PRE_AUTH_RECEIVED" or "PRO_RECEIVED" or "H_AND_P_RECEIVED" or "F2F_RECEIVED" or "PCP_CONFIRMED" or "AVS_RECEIVED" or "SIGNED_CONSENTS" or "INSURANCE_VERIFICATION" => 1,
            "SOC_COMPLIANT" or "DME_MED_SUPPLY" or "NOA_FILED" or "OASIS_SOC_COMPLETE" or "QA_APPROVAL" or "ORDERS_SIGNED" or "DOCS_UPLOADED" or "CASE_MIX_IDENTIFIED" or "CASE_MIX_ORDERS_SIGNED" or "CASE_MIX_PLOTTED" => 2,
            "ROC" or "RECERTIFICATION" or "PCP_PT_NOTIFIED" or "DISCHARGE_SUMMARY_SIGNED" or "NOMNC_SIGNED" => 3,
            _ => 0
        };
    }

    private static List<WorkflowStageResponseDto> BuildWorkflowStages(Patient patient, Referral? referral)
    {
        // Keep the Patient Workflow progress bar aligned with the established
        // CCAP patient workflow presentation. Insurance is shown as the
        // compliance checkpoint, followed by Admission and then SOC.
        // Recertification remains a milestone shown in the progress bar; it is
        // not a separate recertification workflow/module.
        var referralCompleted = referral is not null;
        var insuranceCompleted = IsComplianceCompleted(patient, "INSURANCE_VERIFICATION");
        var socCompleted = IsComplianceCompleted(patient, "SOC_COMPLIANT");

        var admissionRequirementCodes = new[]
        {
            "DME_MED_SUPPLY",
            "NOA_FILED",
            "OASIS_SOC_COMPLETE",
            "QA_APPROVAL",
            "ORDERS_SIGNED",
            "DOCS_UPLOADED",
            "CASE_MIX_IDENTIFIED",
            "CASE_MIX_ORDERS_SIGNED",
            "CASE_MIX_PLOTTED"
        };

        var admissionCompleted = admissionRequirementCodes.All(code => IsComplianceCompleted(patient, code));
        var ongoingCareCompleted = patient.CareCompletedAt.HasValue;
        var recertCompleted = patient.RecertDate.HasValue || IsComplianceCompleted(patient, "RECERTIFICATION");
        var finalOutcomeCompleted = patient.CareCompletedAt.HasValue || patient.TransferDate.HasValue;

        var stages = new List<WorkflowStageResponseDto>
        {
            CreateWorkflowStage(patient, 1, "REFERRAL", "Referral", referralCompleted, referral?.ReferralDate),
            CreateWorkflowStage(patient, 2, "INSURANCE", "Insurance", insuranceCompleted, GetComplianceCompletedDate(patient, "INSURANCE_VERIFICATION")),
            CreateWorkflowStage(patient, 3, "ADMISSION", "Admission", admissionCompleted, admissionCompleted ? GetAdmissionCompletionDate(patient, admissionRequirementCodes) : null),
            CreateWorkflowStage(patient, 4, "SOC", "SOC", socCompleted, GetSocCompletionDate(patient)),
            CreateWorkflowStage(patient, 5, "ONGOING_CARE", "Ongoing Care", ongoingCareCompleted, patient.CareCompletedAt),
            CreateWorkflowStage(patient, 6, "RECERTIFICATION", "Recertification", recertCompleted, patient.RecertDate?.ToDateTime(TimeOnly.MinValue) ?? GetComplianceCompletedDate(patient, "RECERTIFICATION")),
            CreateWorkflowStage(patient, 7, "FINAL_OUTCOME", "Final Outcome", finalOutcomeCompleted, patient.CareCompletedAt ?? patient.TransferDate?.ToDateTime(TimeOnly.MinValue))
        };

        SetWorkflowState(stages);
        return stages;
    }

    private static bool IsComplianceCompleted(Patient patient, string requirementCode)
    {
        return patient.ComplianceRecords.Any(x =>
            x.IsCompleted &&
            string.Equals(x.RequirementCode, requirementCode, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasWorkflowHistory(Patient patient, params string[] terms)
    {
        return patient.Activities.Any(activity =>
            terms.Any(term =>
                activity.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                activity.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                activity.ActivityType.Contains(term, StringComparison.OrdinalIgnoreCase)))
            || patient.Tasks.Any(task =>
                task.Status == PatientTaskStatus.Completed &&
                terms.Any(term =>
                    task.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    task.Description.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private static DateTime? GetWorkflowHistoryDate(Patient patient, params string[] terms)
    {
        var activityDate = patient.Activities
            .Where(activity => terms.Any(term =>
                activity.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                activity.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                activity.ActivityType.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(activity => (DateTime?)activity.ActivityDate)
            .OrderByDescending(x => x)
            .FirstOrDefault();

        var taskDate = patient.Tasks
            .Where(task => task.Status == PatientTaskStatus.Completed && task.CompletedAt.HasValue)
            .Where(task => terms.Any(term =>
                task.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                task.Description.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(task => task.CompletedAt)
            .OrderByDescending(x => x)
            .FirstOrDefault();

        return new[] { activityDate, taskDate }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .OrderByDescending(x => x)
            .Cast<DateTime?>()
            .FirstOrDefault();
    }

    private static DateTime? GetAdmissionCompletionDate(
    Patient patient,
    IEnumerable<string> requirementCodes)
    {
        var completedDates =
            patient.ComplianceRecords
                .Where(x =>
                    x.IsCompleted &&
                    requirementCodes.Any(
                        requirementCode =>
                            string.Equals(
                                x.RequirementCode,
                                requirementCode,
                                StringComparison.OrdinalIgnoreCase)))
                .Select(x => x.CompletedAt)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

        if (completedDates.Count == 0)
            return null;

        return completedDates.Max();
    }

    private static WorkflowStageResponseDto CreateWorkflowStage(Patient patient, int sequence, string stageCode, string stageName, bool completed, DateTime? completedDate = null, bool required = true)
    {
        return new WorkflowStageResponseDto { Sequence=sequence, StageCode=stageCode, StageName=stageName, Status=required ? (completed ? 2 : 0) : 3, Description=required ? (completed ? "Completed" : "Pending") : "Not applicable", CompletedDate=completedDate, AssignedUserId=patient.ClinicianId, AssignedUserName=patient.Clinician is null ? string.Empty : $"{patient.Clinician.FirstName} {patient.Clinician.LastName}", IsClickable=required && completed, Route=$"/tracker/patient/{patient.PatientId}" };
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
        return patient.ComplianceRecords
            .Where(x =>
                string.Equals(
                    x.RequirementCode,
                    "SOC_COMPLIANT",
                    StringComparison.OrdinalIgnoreCase)
                && x.IsCompleted)
            .OrderByDescending(x => x.CompletedAt)
            .Select(x => x.CompletedAt)
            .FirstOrDefault();
    }

    private static void SetWorkflowState(List<WorkflowStageResponseDto> stages)
    {
        var currentIndex = stages.FindIndex(x => x.Status != 2 && x.Status != 3);
        if (currentIndex < 0) return;
        for (var i=0;i<stages.Count;i++)
        {
            var stage=stages[i];
            if(stage.Status==3) continue;
            if(i<currentIndex){stage.Status=2;stage.Description="Completed";stage.IsClickable=true;}
            else if(i==currentIndex){stage.Status=1;stage.Description="Current Stage";stage.IsClickable=true;}
            else{stage.Status=0;stage.Description="Pending";stage.IsClickable=false;}
        }
    }

    private static string GetComplianceRequirementName(
    string requirementCode)
    {
        return requirementCode
            .Trim()
            .ToUpperInvariant() switch
        {
            // =====================================================
            // PHASE 1 - INTAKE & ADMISSION PREPARATION
            // =====================================================

            "PRE_AUTH_RECEIVED" =>
                "Pre-Auth Received",

            "PRO_RECEIVED" =>
                "PRO Received",

            "H_AND_P_RECEIVED" =>
                "H&P",

            "F2F_RECEIVED" =>
                "F2F Received",

            "PCP_CONFIRMED" =>
                "PCP Confirmed",

            "AVS_RECEIVED" =>
                "AVS Received",

            "SIGNED_CONSENTS" =>
                "Signed Consents",

            // =====================================================
            // PHASE 2 - CARE INITIATION & COMPLIANCE
            // =====================================================

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

            "SOC_FEEDBACK_PATIENT" =>
                "SOC Feedback from Patient",

            "CASE_MIX_IDENTIFIED" =>
                "Case Mix Identified",

            "CASE_MIX_ORDERS_SIGNED" =>
                "Case Mix Orders Signed",

            "CASE_MIX_PLOTTED" =>
                "Case Mix Plotted",

            "FREQUENCY_PLOTTED" =>
                "Frequency Plotted",

            "PCP_PT_NOTIFIED" =>
                "PCP & PT Notified",

            // =====================================================
            // PHASE 3 - ONGOING / TRANSITION / DISCHARGE
            // =====================================================

            "TRANSFERS" =>
                "Transfers",

            "ROC" =>
                "ROC",

            "RECERTIFICATION" =>
                "Recertification",

            "DISCHARGE_SUMMARY_SIGNED" =>
                "Discharge Summary Signed",

            "NOMNC_SIGNED" =>
                "NOMNC Signed",


            // =====================================================
            // LEGACY REQUIREMENTS
            // =====================================================
            // These are retained only so existing records don't
            // display raw requirement codes if old records still
            // exist in the database.
            // They are NOT created for new referrals.
            // =====================================================

            "REFERRAL_DOCUMENT" =>
                "Referral Document",

            "INSURANCE_VERIFICATION" =>
                "Insurance Document Verification",

            "PHYSICIAN_ORDERS" =>
                "Physician Orders",

            "SOC_SCHEDULING" =>
                "SOC Scheduling",

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
            // =====================================================
            // PHASE 1
            // =====================================================

            "PRE_AUTH_RECEIVED" => 1,
            "PRO_RECEIVED" => 2,
            "H_AND_P_RECEIVED" => 3,
            "F2F_RECEIVED" => 4,
            "PCP_CONFIRMED" => 5,
            "AVS_RECEIVED" => 6,
            "SIGNED_CONSENTS" => 7,
            "INSURANCE_VERIFICATION" => 8,

            // =====================================================
            // PHASE 2
            // =====================================================

            "SOC_COMPLIANT" => 9,
            "DME_MED_SUPPLY" => 10,
            "NOA_FILED" => 11,
            "OASIS_SOC_COMPLETE" => 12,
            "QA_APPROVAL" => 13,
            "ORDERS_SIGNED" => 14,
            "DOCS_UPLOADED" => 15,
            "SOC_FEEDBACK_PATIENT" => 16,
            "CASE_MIX_IDENTIFIED" => 17,
            "CASE_MIX_ORDERS_SIGNED" => 18,
            "CASE_MIX_PLOTTED" => 19,

            // =====================================================
            // PHASE 3
            // =====================================================

            "TRANSFERS" => 19,
            "ROC" => 20,
            "RECERTIFICATION" => 21,
            "DISCHARGE_SUMMARY_SIGNED" => 22,
            "NOMNC_SIGNED" => 23,
            "PCP_PT_NOTIFIED" => 24,

            // =====================================================
            // LEGACY REQUIREMENTS
            // =====================================================

            "REFERRAL_DOCUMENT" => 90,
            "PHYSICIAN_ORDERS" => 92,
            "SOC_SCHEDULING" => 93,

            _ => 100
        };
    }

}