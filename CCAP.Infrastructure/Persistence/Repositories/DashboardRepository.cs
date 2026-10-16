using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Dashboard.DTOs;
using CCAP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _db;

    private readonly IAnnouncementRepository _announcementRepository;
    public DashboardRepository(AppDbContext db, IAnnouncementRepository announcementRepository)
    {
        _db = db;
        _announcementRepository = announcementRepository;
    }

    public async Task<DashboardDto> GetDashboardAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);

        // =========================================================
        // TASKS — derive outstanding work from the same compliance records
        // that the patient's Compliance tab edits. This prevents the dashboard
        // from showing a separate task that cannot be completed in Compliance.
        // =========================================================
        var activeComplianceRequirementCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PRE_AUTH_RECEIVED", "PRO_RECEIVED", "H_AND_P_RECEIVED", "F2F_RECEIVED",
            "PCP_CONFIRMED", "AVS_RECEIVED", "SIGNED_CONSENTS", "INSURANCE_VERIFICATION",
            "PHYSICIAN_ORDERS", "SOC_COMPLIANT", "DME_MED_SUPPLY", "NOA_FILED", "OASIS_SOC_COMPLETE",
            "QA_APPROVAL", "ORDERS_SIGNED", "DOCS_UPLOADED", "CASE_MIX_IDENTIFIED",
            "CASE_MIX_ORDERS_SIGNED", "CASE_MIX_PLOTTED", "PCP_PT_NOTIFIED", "ROC",
            "RECERTIFICATION", "DISCHARGE_SUMMARY_SIGNED", "NOMNC_SIGNED"
        };

        var complianceRecords = await _db.ComplianceRecords
            .AsNoTracking()
            .Where(x => !x.IsCompleted && x.Patient.Status != PatientStatus.Archived
                && x.Patient.Status != PatientStatus.Completed)
            .Select(x => new
            {
                x.PatientId,
                x.RequirementCode,
                x.Patient.FirstName,
                x.Patient.LastName,
                x.Patient.SocDate,
                Clinician = x.Patient.Clinician == null ? "" : x.Patient.Clinician.FirstName + " " + x.Patient.Clinician.LastName,
                Coordinator = x.Patient.Coordinator == null ? "" : x.Patient.Coordinator.FirstName + " " + x.Patient.Coordinator.LastName
            })
            .ToListAsync(cancellationToken);

        var taskDtos = complianceRecords
            .Where(x => activeComplianceRequirementCodes.Contains(x.RequirementCode))
            .Select(x => new MyTaskDto
            {
                TransactionId = Guid.Empty,
                PatientId = x.PatientId,
                Priority = x.SocDate.HasValue && x.SocDate.Value.ToDateTime(TimeOnly.MinValue) < now ? "High" : "Medium",
                TaskName = ComplianceTaskName(x.RequirementCode),
                PatientName = (x.FirstName + " " + x.LastName).Trim(),
                DueDate = x.SocDate.HasValue ? x.SocDate.Value.ToDateTime(TimeOnly.MinValue) : tomorrow,
                AssignedTo = string.IsNullOrWhiteSpace(x.Coordinator) ? "Unassigned" : x.Coordinator,
                AssignedClinician = string.IsNullOrWhiteSpace(x.Clinician) ? "Unassigned" : x.Clinician,
                CurrentStage = "Compliance"
            })
            .OrderBy(x => x.DueDate)
            .ToList();

        // =========================================================
        // TASK STATISTICS
        // =========================================================

        var myTaskCount = await _db.PatientTasks
            .AsNoTracking()
            .CountAsync(
                x =>
                    x.Status != PatientTaskStatus.Completed &&
                    x.Status != PatientTaskStatus.Cancelled &&
                    (x.AssignedUserId == null ||
                     x.AssignedUserId == userId),
                cancellationToken);

        var highPriorityCount = await _db.PatientTasks
            .AsNoTracking()
            .CountAsync(
                x =>
                    x.Status != PatientTaskStatus.Completed &&
                    x.Status != PatientTaskStatus.Cancelled &&
                    (x.AssignedUserId == null ||
                     x.AssignedUserId == userId) &&
                    x.DueDate < now,
                cancellationToken);

        var dueTodayCount = await _db.PatientTasks
            .AsNoTracking()
            .CountAsync(
                x =>
                    x.Status != PatientTaskStatus.Completed &&
                    x.Status != PatientTaskStatus.Cancelled &&
                    (x.AssignedUserId == null ||
                     x.AssignedUserId == userId) &&
                    x.DueDate >= today &&
                    x.DueDate < tomorrow,
                cancellationToken);

        var completedTodayCount =
            await _db.PatientTasks
                .AsNoTracking()
                .CountAsync(
                    x =>
                        x.Status == PatientTaskStatus.Completed &&
                        x.CompletedAt >= today &&
                        x.CompletedAt < tomorrow &&
                        (x.AssignedUserId == null ||
                         x.AssignedUserId == userId),
                    cancellationToken);

        // =========================================================
        // RECENT REFERRALS
        // =========================================================

        var referrals = await _db.Referrals
            .AsNoTracking()
            .Where(x => x.PatientId != null)
            .OrderByDescending(x => x.ReferralDate)
            .Take(5)
            .Select(x => new ReferralSummaryDto
            {
                PatientName =
                    x.Patient == null
                        ? "Unknown Patient"
                        : x.Patient.FirstName + " " +
                          x.Patient.LastName,

                ReferralNumber = x.ReferralNumber,

                ReferralDate = x.ReferralDate
            })
            .ToListAsync(cancellationToken);

        // =========================================================
        // UPCOMING VISITS
        // =========================================================

        var visits = await _db.Visits
            .AsNoTracking()
            .Where(x =>
                x.Status != "Completed" &&
                x.Status != "Cancelled" &&
                x.ScheduledDate >= now &&
                x.ClinicianId == userId &&
                x.PatientId.HasValue)
            .OrderBy(x => x.ScheduledDate)
            .Take(5)
            .Select(x => new UpcomingVisitDto
            {
                PatientName =
                    x.Patient!.FirstName + " " +
                    x.Patient.LastName,

                Clinician =
                    x.Clinician!.FirstName + " " +
                    x.Clinician.LastName,

                VisitDate = x.ScheduledDate
            })
            .ToListAsync(cancellationToken);

        // =========================================================
        // ANNOUNCEMENTS
        // =========================================================


        var announcements =
            await _announcementRepository.GetActiveAsync(
                cancellationToken);

                var dashboardAnnouncements =
                    announcements
                        .Select(x => new AnnouncementDto
                        {
                            Title = x.Title,
                            Message = x.Message
                        })
                        .ToList();

        // =========================================================
        // RESULT
        // =========================================================

        return new DashboardDto
        {
            Statistics = new WorkStatisticsDto
            {
                MyTasks = taskDtos.Count,
                HighPriority = taskDtos.Count(x => x.Priority == "High"),
                DueToday = taskDtos.Count(x => x.DueDate.Date == today),
                CompletedToday = completedTodayCount
            },

            Tasks = taskDtos,

            RecentReferrals = referrals,

            UpcomingVisits = visits,

            Announcements = dashboardAnnouncements
        };
    }

    private static string ComplianceTaskName(string code) => code switch
    {
        "PRE_AUTH_RECEIVED" => "Confirm Pre-Authorization Received",
        "PRO_RECEIVED" => "Upload Required Documents",
        "H_AND_P_RECEIVED" => "Confirm H&P Received",
        "F2F_RECEIVED" => "Confirm Face-to-Face Documentation",
        "PCP_CONFIRMED" => "Confirm PCP",
        "AVS_RECEIVED" => "Confirm AVS Received",
        "SIGNED_CONSENTS" => "Confirm Signed Consents",
        "INSURANCE_VERIFICATION" => "Complete Insurance Verification",
        "SOC_COMPLIANT" => "Complete SOC Compliance",
        "DME_MED_SUPPLY" => "Confirm DME / Medical Supplies",
        "NOA_FILED" => "Confirm NOA Filed",
        "OASIS_SOC_COMPLETE" => "Complete OASIS / SOC",
        "QA_APPROVAL" => "Confirm QA Approval",
        "ORDERS_SIGNED" => "Confirm Orders Signed",
        "PHYSICIAN_ORDERS" => "Verify Physician Orders",
        "DOCS_UPLOADED" => "Upload Required Documents",
        "CASE_MIX_IDENTIFIED" => "Identify Case Mix",
        "CASE_MIX_ORDERS_SIGNED" => "Confirm Case Mix Orders Signed",
        "CASE_MIX_PLOTTED" => "Confirm Case Mix Plotted",
        "PCP_PT_NOTIFIED" => "Confirm PCP & PT Notified",
        "ROC" => "Confirm ROC",
        "RECERTIFICATION" => "Complete Recertification",
        "DISCHARGE_SUMMARY_SIGNED" => "Complete Compliance: DISCHARGE SUMMARY SIGNED",
        "NOMNC_SIGNED" => "Complete Compliance: NOMNC SIGNED",
        _ => code.Replace('_', ' ')
    };
}