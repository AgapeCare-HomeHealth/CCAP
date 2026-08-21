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
        // TASKS
        // =========================================================

        var tasks = await _db.PatientTasks
            .AsNoTracking()
            .Where(x =>
                x.Status != PatientTaskStatus.Completed &&
                x.Status != PatientTaskStatus.Cancelled &&
                (x.AssignedUserId == null ||
                 x.AssignedUserId == userId))
            .OrderBy(x => x.DueDate)
            .Take(20)
            .Select(x => new
            {
                x.TaskId,
                x.PatientId,
                x.Title,
                x.Description,
                x.DueDate,
                x.PageRoute,
                PatientName =
                    x.Patient.FirstName + " " +
                    x.Patient.LastName,
                AssignedTo =
                    x.AssignedUser == null
                        ? ""
                        : x.AssignedUser.FirstName + " " +
                          x.AssignedUser.LastName
            })
            .ToListAsync(cancellationToken);

        var taskDtos = tasks
            .Select(x =>
            {
                // PatientTask currently has no Priority column.
                // Calculate a dashboard priority from due date.
                var priority =
                    x.DueDate < now
                        ? "High"
                        : x.DueDate < today.AddDays(1)
                            ? "Medium"
                            : "Low";

                return new MyTaskDto
                {
                    TransactionId = x.TaskId,
                    PatientId = x.PatientId,
                    Priority = priority,
                    TaskName = x.Title,
                    PatientName = x.PatientName,
                    DueDate = x.DueDate,
                    AssignedTo = string.IsNullOrWhiteSpace(x.AssignedTo)
                        ? "Unassigned"
                        : x.AssignedTo,
                    CurrentStage = "Patient Workflow"
                };
            })
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
                x.ClinicianId == userId)
            .OrderBy(x => x.ScheduledDate)
            .Take(5)
            .Select(x => new UpcomingVisitDto
            {
                PatientName =
                    x.Patient.FirstName + " " +
                    x.Patient.LastName,

                Clinician =
                    x.Clinician.FirstName + " " +
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

            Announcements = announcements
        };
    }
}