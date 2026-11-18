using System.Security.Claims;
using CCAP.Application.Features.Scheduling.Import;
using CCAP.Application.Features.Scheduling.Queries.GetSchedulingOptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/calendar-import")]
[Authorize(Roles = "Administrator,Admin")]
public sealed class CalendarImportController : ControllerBase
{
    private const long MaximumFileSize = 20 * 1024 * 1024;
    private readonly ISender _sender;

    public CalendarImportController(ISender sender) => _sender = sender;

    [HttpGet("care-coordinators")]
    public async Task<IActionResult> GetCareCoordinators(CancellationToken cancellationToken)
    {
        var options = await _sender.Send(new GetSchedulingOptionsQuery(), cancellationToken);
        return Ok(options.CareCoordinators);
    }

    [HttpPost("preview")]
    [RequestSizeLimit(MaximumFileSize)]
    public async Task<IActionResult> Preview(IFormFile file, [FromForm] Guid careCoordinatorId, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Select a non-empty Excel or CSV schedule file." });
        var extension = Path.GetExtension(file.FileName);
        if (!extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .xlsx Excel or .csv schedule files are supported." });
        if (file.Length > MaximumFileSize) return BadRequest(new { message = "The file must be 20 MB or smaller." });
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        try
        {
            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            return Ok(await _sender.Send(new PreviewCalendarImportCommand(userId, careCoordinatorId, file.FileName, memory.ToArray()), cancellationToken));
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidDataException ex) { return BadRequest(new { message = $"The uploaded file could not be processed: {ex.Message}" }); }
        catch (ArgumentException ex) { return BadRequest(new { message = $"The uploaded file has an invalid format: {ex.Message}" }); }
    }

    [HttpPost("{batchId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid batchId, CancellationToken cancellationToken)
    {
        try { return Accepted(await _sender.Send(new QueueCalendarImportCommand(batchId), cancellationToken)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Calendar import preview was not found." }); }
    }

    [HttpGet("{batchId:guid}")]
    public async Task<IActionResult> Get(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _sender.Send(new GetCalendarImportQuery(batchId), cancellationToken);
        return batch is null ? NotFound() : Ok(batch);
    }

    [HttpGet("{batchId:guid}/logs")]
    public async Task<IActionResult> Logs(Guid batchId, CancellationToken cancellationToken)
    {
        try { return Ok(await _sender.Send(new GetCalendarImportLogsQuery(batchId), cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet]
    public async Task<IActionResult> History(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetCalendarImportHistoryQuery(), cancellationToken));

    [HttpGet("{batchId:guid}/errors")]
    public async Task<IActionResult> DownloadErrors(Guid batchId, CancellationToken cancellationToken)
    {
        var file = await _sender.Send(new DownloadCalendarImportErrorsQuery(batchId), cancellationToken);
        return file is null ? NotFound(new { message = "There are no failed or duplicate schedule rows." }) : File(file.Bytes, "text/csv", file.FileName);
    }

    private bool TryGetCurrentUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out userId);
}
