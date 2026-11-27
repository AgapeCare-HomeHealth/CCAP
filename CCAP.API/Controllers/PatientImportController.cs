using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CCAP.Domain.Entities;
using CCAP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/patient-import")]
[Authorize(Roles = "Administrator")]
public sealed class PatientImportController : ControllerBase
{
    private readonly AppDbContext _db;
    public PatientImportController(AppDbContext db) => _db = db;

    [HttpPost("preview")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Preview(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Select a non-empty CSV file." });
        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase)) return BadRequest(new { message = "Only CSV files are supported." });
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var userId)) return Unauthorized();

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        var csv = await reader.ReadToEndAsync(ct);
        var parsed = Csv.Read(csv);
        if (parsed.Count < 2) return BadRequest(new { message = "The CSV must contain a header and at least one patient row." });
        var headers = parsed[0].Select(x => x.Trim()).ToList();
        var required = new[] { "FirstName", "LastName", "CoordinatorEmail" };
        var missing = required.Where(x => !headers.Contains(x, StringComparer.OrdinalIgnoreCase)).ToArray();
        if (missing.Length > 0) return BadRequest(new { message = $"Missing required columns: {string.Join(", ", missing)}." });

        var users = await _db.ApplicationUsers.Include(x => x.Role).Where(x => x.IsActive).AsNoTracking().ToListAsync(ct);
        var batch = new PatientImportBatch(Path.GetFileName(file.FileName), Hash(csv), parsed.Count - 1, userId);
        var rows = new List<PatientImportRow>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var previousRows = await _db.PatientImportRows.AsNoTracking().Where(x => x.Status == "Inserted" || x.Status == "Failed" || x.Status == "Processing" || x.Status == "Ready").OrderByDescending(x => x.ProcessedAt).ToListAsync(ct);
        var previousByFingerprint = previousRows.GroupBy(x => x.RecordFingerprint, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < parsed.Count; i++)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++) values[headers[c]] = c < parsed[i].Count ? parsed[i][c].Trim() : string.Empty;
            // ImportError is deliberately excluded from the fingerprint so an error report can be corrected and re-uploaded.
            var fingerprintPayload = string.Join("\u001f", headers.Where(h => !h.Equals("ImportError", StringComparison.OrdinalIgnoreCase) && !h.Equals("ImportStatus", StringComparison.OrdinalIgnoreCase)).OrderBy(h => h, StringComparer.OrdinalIgnoreCase).Select(h => values.GetValueOrDefault(h, "").Trim().ToUpperInvariant()));
            var fingerprint = Hash(fingerprintPayload);
            string? error = null;
            if (string.IsNullOrWhiteSpace(values.GetValueOrDefault("FirstName")) || string.IsNullOrWhiteSpace(values.GetValueOrDefault("LastName"))) error = "First Name and Last Name are required.";
            var coordinatorEmail = values.GetValueOrDefault("CoordinatorEmail", "").Trim();
            var coordinator = users.FirstOrDefault(x => x.Email.Equals(coordinatorEmail, StringComparison.OrdinalIgnoreCase) && x.Role.RoleName.Equals("Care Coordinator", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(error) && coordinator is null) error = $"No active Care Coordinator was found for '{coordinatorEmail}'.";
            var clinicianName = values.GetValueOrDefault("AssignedClinician", "").Trim();
            var clinicianMatches = string.IsNullOrWhiteSpace(clinicianName) ? new List<ApplicationUser>() : ResolveClinician(users, clinicianName).ToList();
            if (string.IsNullOrWhiteSpace(error) && !string.IsNullOrWhiteSpace(clinicianName) && clinicianMatches.Count == 0) error = $"No active clinician matched '{clinicianName}'.";
            else if (string.IsNullOrWhiteSpace(error) && clinicianMatches.Count > 1) error = $"Clinician '{clinicianName}' matches more than one active user. Use a unique clinician name.";
            var dobText = values.GetValueOrDefault("DateOfBirth", "").Trim();
            if (string.IsNullOrWhiteSpace(error) && !string.IsNullOrWhiteSpace(dobText) && !DateOnly.TryParse(dobText, out _)) error = $"Date of Birth '{dobText}' is not a valid date.";
            var status = "Ready";
            if (!string.IsNullOrWhiteSpace(error)) status = "Failed";
            else if (!seen.Add(fingerprint)) { status = "Duplicate"; error = "This record appears more than once in the uploaded CSV."; }
            else if (previousByFingerprint.TryGetValue(fingerprint, out var previous) && previous.Status == "Inserted") { status = "Duplicate"; error = "This exact patient data was successfully imported in a previous batch."; }
            var json = JsonSerializer.Serialize(values);
            var importRow = new PatientImportRow(batch.PatientImportBatchId, i + 1, fingerprint, json, error, status);
            PatientImportRow? prior = null;
            if (previousByFingerprint.TryGetValue(fingerprint, out var exactPrior) && exactPrior.Status != "Inserted") prior = exactPrior;
            else if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("FirstName")) && !string.IsNullOrWhiteSpace(values.GetValueOrDefault("LastName")))
            {
                prior = previousRows.FirstOrDefault(x => x.Status == "Failed" && SameRetryIdentity(x.RawDataJson, values));
            }
            if (prior is not null)
            {
                importRow.UseIdentifiers(prior.GeneratedMrn, prior.GeneratedReferralNumber);
                importRow.UseCreatedRecords(prior.PatientId, prior.ReferralId);
            }
            rows.Add(importRow);
        }

        batch.RefreshCounts(0, rows.Count(x => x.Status == "Duplicate"), rows.Count(x => x.Status == "Failed"), rows.Count);
        _db.PatientImportBatches.Add(batch);
        _db.PatientImportRows.AddRange(rows);
        await _db.SaveChangesAsync(ct);
        return Ok(await GetBatchResult(batch.PatientImportBatchId, ct));
    }

    [HttpPost("{batchId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid batchId, CancellationToken ct)
    {
        var batch = await _db.PatientImportBatches.FirstOrDefaultAsync(x => x.PatientImportBatchId == batchId, ct);
        if (batch is null) return NotFound();
        if (batch.Status != "Preview") return Conflict(new { message = "This import has already been queued or processed." });
        var rows = await _db.PatientImportRows.Where(x => x.PatientImportBatchId == batchId).ToListAsync(ct);
        if (!rows.Any(x => x.Status == "Ready")) return BadRequest(new { message = "There are no valid records to import." });
        batch.Queue();
        await _db.SaveChangesAsync(ct);
        return Accepted(await GetBatchSummary(batchId, ct));
    }

    [HttpGet("{batchId:guid}")]
    public async Task<IActionResult> Get(Guid batchId, CancellationToken ct)
    {
        var result = await GetBatchSummary(batchId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{batchId:guid}/logs")]
    public async Task<IActionResult> Logs(Guid batchId, CancellationToken ct)
    {
        var exists = await _db.PatientImportBatches.AnyAsync(x => x.PatientImportBatchId == batchId, ct);
        if (!exists) return NotFound();
        var rows = await _db.PatientImportRows.AsNoTracking().Where(x => x.PatientImportBatchId == batchId).OrderBy(x => x.RowNumber)
            .Select(x => new { x.PatientImportRowId, x.RowNumber, x.Status, x.ErrorMessage }).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet]
    public async Task<IActionResult> History(CancellationToken ct)
    {
        var batches = await (from b in _db.PatientImportBatches.AsNoTracking()
                             join u in _db.ApplicationUsers.AsNoTracking() on b.UploadedByUserId equals u.UserId into users
                             from u in users.DefaultIfEmpty()
                             orderby b.UploadedAt descending
                             select new { b.PatientImportBatchId, b.FileName, b.Status, b.TotalRows, b.InsertedRows, b.DuplicateRows, b.FailedRows, b.UploadedAt, b.StartedAt, b.CompletedAt, b.Summary,
                                 UploadedBy = u == null ? "Unknown" : u.FirstName + " " + u.LastName }).Take(100).ToListAsync(ct);
        return Ok(batches);
    }

    [HttpGet("{batchId:guid}/errors")]
    public async Task<IActionResult> DownloadErrors(Guid batchId, CancellationToken ct)
    {
        var rows = await _db.PatientImportRows.Where(x => x.PatientImportBatchId == batchId && (x.Status == "Failed" || x.Status == "Duplicate")).OrderBy(x => x.RowNumber).ToListAsync(ct);
        if (rows.Count == 0) return NotFound(new { message = "There are no failed or skipped records." });
        var allKeys = new List<string>();
        var dictionaries = rows.Select(x => JsonSerializer.Deserialize<Dictionary<string, string>>(x.RawDataJson) ?? new()).ToList();
        foreach (var key in dictionaries.SelectMany(x => x.Keys)) if (!allKeys.Contains(key, StringComparer.OrdinalIgnoreCase)) allKeys.Add(key);
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", allKeys.Append("ImportStatus").Append("ImportError").Select(Csv.Escape)));
        for (var i = 0; i < rows.Count; i++)
        {
            var values = dictionaries[i];
            builder.AppendLine(string.Join(",", allKeys.Select(k => Csv.Escape(values.GetValueOrDefault(k, ""))).Append(Csv.Escape(rows[i].Status)).Append(Csv.Escape(rows[i].ErrorMessage ?? "Not inserted"))));
        }
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", $"PatientImport_{batchId:N}_errors.csv");
    }

    private async Task<object?> GetBatchSummary(Guid id, CancellationToken ct)
    {
        var batch = await _db.PatientImportBatches.AsNoTracking().FirstOrDefaultAsync(x => x.PatientImportBatchId == id, ct);
        if (batch is null) return null;
        var uploader = await _db.ApplicationUsers.AsNoTracking().Where(x => x.UserId == batch.UploadedByUserId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefaultAsync(ct);
        return new { batch.PatientImportBatchId, batch.FileName, batch.Status, batch.TotalRows, batch.InsertedRows, batch.DuplicateRows, batch.FailedRows, batch.UploadedAt, batch.StartedAt, batch.CompletedAt, batch.Summary, UploadedBy = uploader ?? "Unknown" };
    }

    private async Task<object?> GetBatchResult(Guid id, CancellationToken ct)
    {
        var batch = await _db.PatientImportBatches.AsNoTracking().FirstOrDefaultAsync(x => x.PatientImportBatchId == id, ct);
        if (batch is null) return null;
        var rows = await _db.PatientImportRows.AsNoTracking().Where(x => x.PatientImportBatchId == id).OrderBy(x => x.RowNumber).ToListAsync(ct);
        var uploader = await _db.ApplicationUsers.AsNoTracking().Where(x => x.UserId == batch.UploadedByUserId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefaultAsync(ct);
        return new { batch.PatientImportBatchId, batch.FileName, batch.Status, batch.TotalRows, batch.InsertedRows, batch.DuplicateRows, batch.FailedRows, batch.UploadedAt, batch.StartedAt, batch.CompletedAt, batch.Summary,
            UploadedBy = uploader ?? "Unknown", Rows = rows.Select(x => new { x.PatientImportRowId, x.RowNumber, x.Status, x.ErrorMessage }) };
    }

    private static bool SameRetryIdentity(string rawJson, IReadOnlyDictionary<string, string> current)
    {
        try
        {
            var prior = JsonSerializer.Deserialize<Dictionary<string, string>>(rawJson) ?? new();
            string Get(IReadOnlyDictionary<string, string> data, string key) => data.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value?.Trim() ?? "";
            return Get(prior, "FirstName").Equals(Get(current, "FirstName"), StringComparison.OrdinalIgnoreCase) &&
                   Get(prior, "LastName").Equals(Get(current, "LastName"), StringComparison.OrdinalIgnoreCase) &&
                   Get(prior, "CoordinatorEmail").Equals(Get(current, "CoordinatorEmail"), StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    internal static IEnumerable<ApplicationUser> ResolveClinician(IEnumerable<ApplicationUser> users, string displayName)
    {
        var tokens = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0) return [];
        var eligible = users.Where(x => !x.Role.RoleName.Contains("Coordinator", StringComparison.OrdinalIgnoreCase) &&
            !x.Role.RoleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase) &&
            (x.Role.RoleName.Contains("Clinician", StringComparison.OrdinalIgnoreCase) ||
             x.Role.RoleName.Contains("Nurse", StringComparison.OrdinalIgnoreCase) || x.DisciplineId.HasValue)).ToList();
        var exact = eligible.Where(x => string.Equals($"{x.FirstName} {x.LastName}", displayName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.FirstName, displayName, StringComparison.OrdinalIgnoreCase)).ToList();
        return exact.Count > 0 ? exact : eligible.Where(x => string.Equals(x.FirstName, tokens[0], StringComparison.OrdinalIgnoreCase));
    }

    internal static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static class Csv
    {
        public static List<List<string>> Read(string input)
        {
            var result = new List<List<string>>(); var row = new List<string>(); var cell = new StringBuilder(); var quoted = false;
            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                if (quoted)
                {
                    if (ch == '"' && i + 1 < input.Length && input[i + 1] == '"') { cell.Append('"'); i++; }
                    else if (ch == '"') quoted = false; else cell.Append(ch);
                }
                else if (ch == '"') quoted = true;
                else if (ch == ',') { row.Add(cell.ToString()); cell.Clear(); }
                else if (ch == '\n') { row.Add(cell.ToString().TrimEnd('\r')); cell.Clear(); result.Add(row); row = []; }
                else cell.Append(ch);
            }
            if (cell.Length > 0 || row.Count > 0) { row.Add(cell.ToString().TrimEnd('\r')); result.Add(row); }
            return result.Where(x => x.Any(v => !string.IsNullOrWhiteSpace(v))).ToList();
        }
        public static string Escape(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
    }
}
