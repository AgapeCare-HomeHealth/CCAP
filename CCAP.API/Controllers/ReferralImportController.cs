using System.Security.Claims;
using System.Text.Json;
using CCAP.Domain.Entities;
using CCAP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/referral-import")]
[Authorize(Roles = "Administrator")]
public sealed class ReferralImportController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReferralImportController(AppDbContext db) => _db = db;

    [HttpPost("history")]
    public async Task<IActionResult> SaveHistory([FromBody] SaveReferralImportRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var userId))
            return Unauthorized();

        var batch = new ReferralImportBatch(
            Path.GetFileName(request.FileName ?? "ReferralImport.csv"),
            userId,
            request.Rows.Count,
            request.Rows.Count(x => x.Status == "Inserted"),
            request.Rows.Count(x => x.Status == "Duplicate"),
            request.Rows.Count(x => x.Status == "Failed" || x.Status == "Validation Error"));

        _db.ReferralImportBatches.Add(batch);
        foreach (var row in request.Rows)
        {
            _db.ReferralImportRows.Add(new ReferralImportRow(
                batch.ReferralImportBatchId,
                row.RowNumber,
                row.PatientName ?? "",
                row.Status ?? "Failed",
                row.ErrorMessage,
                row.RawDataJson ?? "{}"));
        }
        await _db.SaveChangesAsync(ct);
        return Ok(new { batch.ReferralImportBatchId });
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(CancellationToken ct)
    {
        var result = await (from b in _db.ReferralImportBatches.AsNoTracking()
                            join u in _db.ApplicationUsers.AsNoTracking() on b.UploadedByUserId equals u.UserId into users
                            from u in users.DefaultIfEmpty()
                            orderby b.UploadedAt descending
                            select new
                            {
                                b.ReferralImportBatchId, b.FileName, b.UploadedAt, b.TotalRows,
                                b.InsertedRows, b.DuplicateRows, b.FailedRows, b.Status,
                                UploadedBy = u == null ? "Unknown" : u.FirstName + " " + u.LastName
                            }).Take(100).ToListAsync(ct);
        return Ok(result);
    }

    [HttpGet("history/{batchId:guid}")]
    public async Task<IActionResult> Logs(Guid batchId, CancellationToken ct)
    {
        var batch = await _db.ReferralImportBatches.AsNoTracking()
            .Where(x => x.ReferralImportBatchId == batchId)
            .Select(x => new { x.ReferralImportBatchId, x.FileName, x.UploadedAt, x.TotalRows, x.InsertedRows, x.DuplicateRows, x.FailedRows, x.Status })
            .FirstOrDefaultAsync(ct);
        if (batch is null) return NotFound();
        var rows = await _db.ReferralImportRows.AsNoTracking()
            .Where(x => x.ReferralImportBatchId == batchId).OrderBy(x => x.RowNumber)
            .Select(x => new { x.ReferralImportRowId, x.RowNumber, x.PatientName, x.Status, x.ErrorMessage, x.RawDataJson })
            .ToListAsync(ct);
        return Ok(new
        {
            batch.ReferralImportBatchId, batch.FileName, batch.UploadedAt, batch.TotalRows,
            batch.InsertedRows, batch.DuplicateRows, batch.FailedRows, batch.Status, Rows = rows
        });
    }
}

public sealed class SaveReferralImportRequest
{
    public string? FileName { get; set; }
    public List<SaveReferralImportRowRequest> Rows { get; set; } = [];
}
public sealed class SaveReferralImportRowRequest
{
    public int RowNumber { get; set; }
    public string? PatientName { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawDataJson { get; set; }
}
