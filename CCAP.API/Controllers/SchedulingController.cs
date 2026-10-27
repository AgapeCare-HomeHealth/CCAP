using CCAP.API.Authorization;
using CCAP.Application.Abstractions.Scheduling;
using CCAP.Application.Features.Scheduling.Import;
using CCAP.Application.Features.Scheduling.Commands.AddSchedules;
using CCAP.Application.Features.Scheduling.Commands.UpdateVisitStatus;
using CCAP.Application.Features.Scheduling.Commands.UpdateCalendarVisitDetails;
using CCAP.Application.Features.Scheduling.Queries.GetCalendarVisits;
using CCAP.Application.Features.Scheduling.Queries.GetSchedulingOptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Claims;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/scheduling")]
public sealed class SchedulingController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IScheduleImportService _importService;

    public SchedulingController(ISender sender, IScheduleImportService importService)
    {
        _sender = sender;
        _importService = importService;
    }

    [HttpGet("calendar")]
    [Authorize(Policy = PermissionPolicies.SchedulingView)]
    public async Task<IActionResult> GetCalendar([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        if (endDate <= startDate) return BadRequest("endDate must be after startDate.");

        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return Ok(await _sender.Send(
            new GetCalendarVisitsQuery(
                userId,
                startDate,
                endDate,
                CanViewAllSchedules(),
                User.IsInRole("Care Coordinator")),
            cancellationToken));
    }

    public sealed record UpdateTrackerDetailsRequest(string ConfirmationStatus, string? CallNotes, Guid? ClinicianId);
    public sealed record UpdateCalendarVisitDetailsRequest(
        string TimeBlock,
        string ConfirmationStatus,
        string? CallNotes,
        Guid? ClinicianId,
        string? NotesFlag,
        string Status);
    public sealed record AssignCareCoordinatorRequest(Guid CareCoordinatorId, List<Guid> VisitIds);

    [HttpPut("visits/{visitId:guid}/details")]
    [Authorize]
    public async Task<IActionResult> UpdateCalendarVisitDetails(
        Guid visitId,
        [FromBody] UpdateCalendarVisitDetailsRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanManageSchedulingOrCareCoordinator()) return Forbid();
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        try
        {
            await _sender.Send(new UpdateCalendarVisitDetailsCommand(
                userId,
                visitId,
                request.TimeBlock,
                request.ConfirmationStatus,
                request.CallNotes,
                request.ClinicianId,
                request.NotesFlag,
                request.Status,
                CanViewAllSchedules(),
                User.IsInRole("Care Coordinator"),
                User.IsInRole("Administrator") || User.IsInRole("Admin")), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Visit not found or not accessible." }); }
    }

    [HttpPut("visits/{visitId:guid}/tracker-details")]
    [Authorize(Policy = PermissionPolicies.SchedulingManage)]
    public async Task<IActionResult> UpdateTrackerDetails(Guid visitId, [FromBody] UpdateTrackerDetailsRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        try
        {
            await _sender.Send(new CCAP.Application.Features.Scheduling.Commands.UpdateTrackerDetails.UpdateTrackerDetailsCommand(
                userId, visitId, request.ConfirmationStatus, request.CallNotes, request.ClinicianId,
                User.IsInRole("Administrator") || User.IsInRole("Admin")), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Visit not found." }); }
    }

    [HttpPost("care-coordinators/assign")]
    [Authorize(Policy = PermissionPolicies.SchedulingManage)]
    public async Task<IActionResult> AssignCareCoordinators([FromBody] AssignCareCoordinatorRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        try
        {
            var assigned = await _sender.Send(new CCAP.Application.Features.Scheduling.Commands.AssignCareCoordinators.AssignCareCoordinatorsCommand(
                userId, request.CareCoordinatorId, request.VisitIds), cancellationToken);
            return Ok(new { assigned });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Selected visits were not found." }); }
    }

    public sealed record DeleteScheduleVisitsRequest(List<Guid> VisitIds);

    [HttpPost("visits/bulk-delete")]
    [Authorize]
    public async Task<IActionResult> DeleteVisits([FromBody] DeleteScheduleVisitsRequest request, CancellationToken cancellationToken)
    {
        if (!CanManageSchedulingOrCareCoordinator()) return Forbid();
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        if (request?.VisitIds is null) return BadRequest(new { message = "Select visits to delete." });
        try
        {
            var deleted = await _sender.Send(new CCAP.Application.Features.Scheduling.Commands.DeleteScheduleVisits.DeleteScheduleVisitsCommand(
                userId, request.VisitIds, User.IsInRole("Administrator") || User.IsInRole("Admin")), cancellationToken);
            return Ok(new { deleted });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Selected visits were not found or are not accessible." }); }
    }

    public sealed record UpdateVisitStatusRequest(string Status);
    public sealed record RescheduleVisitRequest(DateTime Date);

    [HttpPut("visits/{visitId:guid}/reschedule")]
    [Authorize(Policy = PermissionPolicies.SchedulingManage)]
    public async Task<IActionResult> RescheduleVisit(Guid visitId, [FromBody] RescheduleVisitRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        try
        {
            await _sender.Send(new CCAP.Application.Features.Scheduling.Commands.RescheduleVisit.RescheduleVisitCommand(
                userId, visitId, request.Date, CanViewAllSchedules(), User.IsInRole("Care Coordinator"),
                User.IsInRole("Administrator") || User.IsInRole("Admin")), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Visit not found or not accessible." }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("visits/{visitId:guid}/status")]
    [Authorize(Policy = PermissionPolicies.SchedulingManage)]
    public async Task<IActionResult> UpdateVisitStatus(Guid visitId, [FromBody] UpdateVisitStatusRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        try
        {
            await _sender.Send(new UpdateVisitStatusCommand(
                userId, visitId, request.Status, User.IsInRole("Administrator") || User.IsInRole("Admin")), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Visit not found." }); }
    }

    [HttpGet("options")]
    [Authorize(Policy = PermissionPolicies.SchedulingView)]
    public async Task<IActionResult> GetOptions(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetSchedulingOptionsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.SchedulingManage)]
    public async Task<IActionResult> AddSchedules(
        [FromBody] List<AddScheduleItem> schedules,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        if (schedules is null || schedules.Count == 0)
            return BadRequest(new { message = "Select at least one schedule date." });

        // A Care Coordinator can only schedule for themselves. Do not trust a
        // client-supplied CC id to grant access to another coordinator's data.
        if (User.IsInRole("Care Coordinator"))
            schedules = schedules.Select(x => x with { CareCoordinatorId = userId }).ToList();

        try
        {
            var result = await _sender.Send(
                new AddSchedulesCommand(userId, schedules),
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var detail = ex.InnerException?.Message;
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = string.IsNullOrWhiteSpace(detail)
                    ? "The schedule could not be saved to the database."
                    : $"The schedule could not be saved to the database: {detail}"
            });
        }
    }

    [HttpPost("import/preview")]
    [Authorize(Policy = PermissionPolicies.SchedulingManage)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> PreviewImport(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest("Please select an Excel or CSV schedule file.");
        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx Excel or .csv schedule files are supported.");

        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        await using var stream = file.OpenReadStream();

        try
        {
            var preview = await _importService.PreviewAsync(
                userId,
                stream,
                file.FileName,
                cancellationToken);

            return Ok(preview);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(new
            {
                message = $"The uploaded file could not be processed: {ex.Message}"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = $"The uploaded file has an invalid format: {ex.Message}"
            });
        }
    }

    [HttpPost("import/commit")]
    [Authorize(Policy = PermissionPolicies.SchedulingManage)]
    public async Task<IActionResult> CommitImport([FromBody] List<ScheduleImportCommitItem> items, CancellationToken cancellationToken)
    {
        if (items is null || items.Count == 0) return BadRequest("Select at least one schedule to import.");

        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return Ok(await _importService.CommitAsync(userId, items, cancellationToken));
    }

    private bool CanViewAllSchedules() =>
        User.IsInRole("Administrator") || User.IsInRole("Admin") || User.IsInRole("Scheduler");

    private bool CanManageSchedulingOrCareCoordinator() =>
        User.HasClaim("permission", PermissionPolicies.SchedulingManage) || User.IsInRole("Care Coordinator");

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
