using System.Security.Claims;
using CCAP.API.Authorization;
using CCAP.API.Controllers.Models;
using CCAP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = PermissionPolicies.NotificationsView)]
public sealed class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> Get(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var now = DateTime.UtcNow;
        var horizon = now.AddDays(3);

        var tasks = await _db.PatientTasks
            .AsNoTracking()
            .Where(x => x.Status != CCAP.Domain.Enums.PatientTaskStatus.Completed &&
                        x.Status != CCAP.Domain.Enums.PatientTaskStatus.Cancelled &&
                        (x.AssignedUserId == null || x.AssignedUserId == userId) &&
                        x.DueDate <= horizon)
            .Select(x => new NotificationResponse
            {
                NotificationId = x.TaskId,
                PatientId = x.PatientId,
                PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                Type = "Task",
                Title = x.Title,
                Message = x.Description,
                Severity = x.DueDate < now ? "Critical" : "Warning",
                DueDate = x.DueDate,
                CreatedAt = x.DueDate
            })
            .ToListAsync(cancellationToken);

        var visits = await _db.Visits
            .AsNoTracking()
            .Where(x => x.Status != "Completed" &&
                        x.Status != "Cancelled" &&
                        x.ClinicianId == userId &&
                        x.ScheduledDate <= horizon)
            .Select(x => new NotificationResponse
            {
                NotificationId = x.VisitId,
                PatientId = x.PatientId,
                PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                Type = "Visit",
                Title = "Upcoming patient visit",
                Message = "A scheduled visit requires attention.",
                Severity = x.ScheduledDate < now ? "Critical" : "Info",
                DueDate = x.ScheduledDate,
                CreatedAt = x.ScheduledDate
            })
            .ToListAsync(cancellationToken);

        return Ok(tasks
            .Concat(visits)
            .OrderBy(x => x.Severity == "Critical" ? 0 : x.Severity == "Warning" ? 1 : 2)
            .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
            .Take(50)
            .ToList());
    }
}
