using CCAP.API.Authorization;
using CCAP.Application.Abstractions.Scheduling;
using CCAP.Application.Features.Scheduling.Import;
using CCAP.Application.Features.Scheduling.Commands.AddSchedules;
using CCAP.Application.Features.Scheduling.Queries.GetCalendarVisits;
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
    [Authorize(Policy = PermissionPolicies.PatientsView)]
    public async Task<IActionResult> GetCalendar([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        if (endDate <= startDate) return BadRequest("endDate must be after startDate.");

        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return Ok(await _sender.Send(
            new GetCalendarVisitsQuery(userId, startDate, endDate),
            cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> AddSchedules(
        [FromBody] List<AddScheduleItem> schedules,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

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
    }

    [HttpPost("import/preview")]
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
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
    [Authorize(Policy = PermissionPolicies.PatientsManage)]
    public async Task<IActionResult> CommitImport([FromBody] List<ScheduleImportCommitItem> items, CancellationToken cancellationToken)
    {
        if (items is null || items.Count == 0) return BadRequest("Select at least one schedule to import.");

        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return Ok(await _importService.CommitAsync(userId, items, cancellationToken));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }
}
