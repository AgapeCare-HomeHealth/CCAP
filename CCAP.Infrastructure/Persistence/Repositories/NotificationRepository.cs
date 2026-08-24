using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Notifications.DTOs;
using CCAP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository
    : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var horizon = now.AddDays(3);

        var tasks = await _db.PatientTasks
            .AsNoTracking()
            .Where(x =>
                x.Status != PatientTaskStatus.Completed &&
                x.Status != PatientTaskStatus.Cancelled &&
                (x.AssignedUserId == null ||
                 x.AssignedUserId == userId) &&
                x.DueDate <= horizon)
            .Select(x => new NotificationDto(
                x.TaskId,
                x.PatientId,
                x.Patient.FirstName + " " +
                    x.Patient.LastName,
                "Task",
                x.Title,
                x.Description,
                x.DueDate < now
                    ? "Critical"
                    : "Warning",
                x.DueDate,
                x.DueDate))
            .ToListAsync(cancellationToken);

        var visits = await _db.Visits
            .AsNoTracking()
            .Where(x =>
                x.Status != "Completed" &&
                x.Status != "Cancelled" &&
                x.ClinicianId == userId &&
                x.ScheduledDate <= horizon)
            .Select(x => new NotificationDto(
                x.VisitId,
                x.PatientId,
                x.Patient.FirstName + " " +
                    x.Patient.LastName,
                "Visit",
                "Upcoming patient visit",
                "A scheduled visit requires attention.",
                x.ScheduledDate < now
                    ? "Critical"
                    : "Info",
                x.ScheduledDate,
                x.ScheduledDate))
            .ToListAsync(cancellationToken);

        return tasks
            .Concat(visits)
            .OrderBy(x =>
                x.Severity == "Critical"
                    ? 0
                    : x.Severity == "Warning"
                        ? 1
                        : 2)
            .ThenBy(x =>
                x.DueDate ?? DateTime.MaxValue)
            .Take(50)
            .ToList();
    }
}