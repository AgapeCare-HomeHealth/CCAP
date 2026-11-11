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

        // Determine dashboard visibility from the application's own role table.
        // Administrators can review all active patient tasks; other users see only
        // tasks assigned to them (plus unassigned tasks for their own patients).
        var roleName = await _db.ApplicationUsers
            .Where(x => x.UserId == userId)
            .Select(x => x.Role.RoleName)
            .FirstOrDefaultAsync(cancellationToken);

        var isAdministrator = string.Equals(
            roleName, "Administrator", StringComparison.OrdinalIgnoreCase);

        var visibleOpenTasks = _db.PatientTasks
            .AsNoTracking()
            .Where(x => x.Status != PatientTaskStatus.Completed
                && x.Status != PatientTaskStatus.Cancelled
                && (isAdministrator
                    || (x.Patient.Status != PatientStatus.Archived
                        && x.Patient.Status != PatientStatus.Completed))
                && (isAdministrator
                    || x.AssignedUserId == userId
                    || (x.AssignedUserId == null && x.Patient.CoordinatorId == userId)));

        var taskDtos = await visibleOpenTasks
            .OrderBy(x => x.DueDate)
            .Select(x => new MyTaskDto
            {
                TransactionId = x.TaskId,
                PatientId = x.PatientId,
                Priority = x.DueDate < now ? "High" : "Medium",
                TaskName = x.Title,
                PatientName = (x.Patient.FirstName + " " + x.Patient.LastName).Trim(),
                DueDate = x.DueDate,
                AssignedTo = x.AssignedUser == null
                    ? (x.Patient.Coordinator == null ? "Unassigned" : x.Patient.Coordinator.FirstName + " " + x.Patient.Coordinator.LastName)
                    : x.AssignedUser.FirstName + " " + x.AssignedUser.LastName,
                AssignedClinician = x.Patient.Clinician == null ? "Unassigned" : x.Patient.Clinician.FirstName + " " + x.Patient.Clinician.LastName,
                CurrentStage = "Compliance",
                PatientCoordinatorRole = isAdministrator
                    ? "Admin View"
                    : x.AssignedUserId == x.Patient.CoordinatorId
                        ? "Primary CC"
                        : x.AssignedUserId == x.Patient.SecondaryCoordinatorId
                            ? "Secondary CC"
                            : x.AssignedUserId == x.Patient.BackupCoordinatorId
                                ? "Backup CC"
                                : "Assigned User"
            })
            .ToListAsync(cancellationToken);

        // Statistics use exactly the same visibility and active-patient scope as the task list.
        var myTaskCount = await visibleOpenTasks.CountAsync(cancellationToken);
        var highPriorityCount = await visibleOpenTasks.CountAsync(x => x.DueDate < now, cancellationToken);
        var dueTodayCount = await visibleOpenTasks.CountAsync(
            x => x.DueDate >= today && x.DueDate < tomorrow, cancellationToken);

        var completedTasks = _db.PatientTasks
            .AsNoTracking()
            .Where(x => x.Status == PatientTaskStatus.Completed
                && x.CompletedAt >= today
                && x.CompletedAt < tomorrow
                && (isAdministrator
                    || (x.Patient.Status != PatientStatus.Archived
                        && x.Patient.Status != PatientStatus.Completed))
                && (isAdministrator
                    || x.AssignedUserId == userId
                    || (x.AssignedUserId == null && x.Patient.CoordinatorId == userId)));

        var completedTodayCount = await completedTasks.CountAsync(cancellationToken);

        // =========================================================
        // RECENT REFERRALS
        // =========================================================

        var referrals = await _db.Referrals
            .AsNoTracking()
            .Where(x => x.PatientId != null)
            .OrderByDescending(x => x.ReferralDate)
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
                            AnnouncementId = x.AnnouncementId,
                            Title = x.Title,
                            Message = x.Message,
                            PublishedAt = x.PublishedAt,
                            ExpiresAt = x.ExpiresAt
                        })
                        .ToList();

        // =========================================================
        // RESULT
        // =========================================================

        return new DashboardDto
        {
            Statistics = new WorkStatisticsDto
            {
                MyTasks = myTaskCount,
                HighPriority = highPriorityCount,
                DueToday = dueTodayCount,
                CompletedToday = completedTodayCount
            },

            Tasks = taskDtos,

            RecentReferrals = referrals,

            UpcomingVisits = visits,

            Announcements = dashboardAnnouncements
        };
    }

}
