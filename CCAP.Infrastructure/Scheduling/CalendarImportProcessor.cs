using System.Text.Json;
using CCAP.Application.Abstractions.Scheduling;
using CCAP.Application.Features.Scheduling.Import;
using CCAP.Domain.Entities;
using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Scheduling;

public sealed class CalendarImportProcessor : ICalendarImportProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _db;

    public CalendarImportProcessor(AppDbContext db) => _db = db;

    public async Task<bool> ProcessNextBatchAsync(CancellationToken cancellationToken)
    {
        var batch = await _db.CalendarImportBatches
            .Where(x => x.Status == "Queued" || x.Status == "Processing")
            .OrderBy(x => x.UploadedAt).FirstOrDefaultAsync(cancellationToken);
        if (batch is null) return false;

        batch.Start();
        await _db.SaveChangesAsync(cancellationToken);
        var rowIds = await _db.CalendarImportRows.Where(x => x.CalendarImportBatchId == batch.CalendarImportBatchId &&
                (x.Status == "Ready" || x.Status == "Processing"))
            .OrderBy(x => x.SheetName).ThenBy(x => x.RowNumber)
            .Select(x => x.CalendarImportRowId).ToListAsync(cancellationToken);

        foreach (var rowId in rowIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            batch = await ProcessRowAsync(batch, rowId, cancellationToken);
        }

        await RefreshCountsAsync(batch, cancellationToken);
        return true;
    }

    private async Task<CalendarImportBatch> ProcessRowAsync(CalendarImportBatch batch, Guid rowId, CancellationToken cancellationToken)
    {
        var row = await _db.CalendarImportRows.FirstAsync(x => x.CalendarImportRowId == rowId, cancellationToken);
        try
        {
            row.MarkProcessing();
            await _db.SaveChangesAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<CalendarImportRowDto>(row.RawDataJson, JsonOptions)
                ?? throw new InvalidOperationException("The saved import row could not be read.");
            if (!data.ScheduledDate.HasValue) throw new InvalidOperationException("Date is required.");
            if (string.IsNullOrWhiteSpace(data.PatientName)) throw new InvalidOperationException("Patient Name is required.");
            if (string.IsNullOrWhiteSpace(data.TimeBlock)) throw new InvalidOperationException("Time Block is required.");
            if (string.IsNullOrWhiteSpace(data.ConfirmationStatus)) throw new InvalidOperationException("Confirmation Status is required.");
            if (string.IsNullOrWhiteSpace(data.VisitType)) throw new InvalidOperationException("Type of Visit is required.");

            var date = data.ScheduledDate.Value.Date;
            var existingOnDate = await _db.Visits.AsNoTracking()
                .Where(x => x.ScheduledDate >= date && x.ScheduledDate < date.AddDays(1))
                .ToListAsync(cancellationToken);
            if (existingOnDate.Any(x => !string.Equals(x.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                SamePatient(x.PatientId, x.PatientName, data.PatientId, data.PatientName) &&
                NormalizeTimeBlock(x.TimeBlock) == NormalizeTimeBlock(data.TimeBlock) &&
                NormalizeToken(x.VisitType) == NormalizeToken(data.VisitType)))
            {
                row.MarkDuplicate("An exact patient, date, time, and visit type already exists. This row was skipped.");
                batch.RefreshCounts(batch.InsertedRows, batch.DuplicateRows + 1, batch.FailedRows, batch.TotalRows);
                await _db.SaveChangesAsync(cancellationToken);
                return batch;
            }

            var patient = data.PatientId.HasValue
                ? await _db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.PatientId == data.PatientId.Value, cancellationToken)
                : null;
            var visit = Visit.CreateImportedSchedule(
                data.PatientName,
                date,
                data.TimeBlock,
                data.ConfirmationStatus,
                data.CallNotes,
                data.ClinicianName,
                patient?.PatientId,
                data.ClinicianId,
                batch.UploadedByUserId,
                data.VisitType,
                string.IsNullOrWhiteSpace(data.VisitStatus) ? "Scheduled" : data.VisitStatus,
                data.NotesFlag,
                batch.CareCoordinatorId ?? patient?.CoordinatorId);
            _db.Visits.Add(visit);
            row.MarkInserted();
            batch.RefreshCounts(batch.InsertedRows + 1, batch.DuplicateRows, batch.FailedRows, batch.TotalRows);
            await _db.SaveChangesAsync(cancellationToken);
            return batch;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _db.ChangeTracker.Clear();
            var failedRow = await _db.CalendarImportRows.FirstAsync(x => x.CalendarImportRowId == rowId, cancellationToken);
            var freshBatch = await _db.CalendarImportBatches.FirstAsync(x => x.CalendarImportBatchId == batch.CalendarImportBatchId, cancellationToken);
            failedRow.MarkFailed(ex.Message);
            freshBatch.RefreshCounts(freshBatch.InsertedRows, freshBatch.DuplicateRows, freshBatch.FailedRows + 1, freshBatch.TotalRows);
            await _db.SaveChangesAsync(cancellationToken);
            return freshBatch;
        }
    }

    private async Task RefreshCountsAsync(CalendarImportBatch batch, CancellationToken cancellationToken)
    {
        var counts = await _db.CalendarImportRows.Where(x => x.CalendarImportBatchId == batch.CalendarImportBatchId)
            .GroupBy(_ => 1).Select(g => new
            {
                Inserted = g.Count(x => x.Status == "Inserted"),
                Duplicates = g.Count(x => x.Status == "Duplicate"),
                Failed = g.Count(x => x.Status == "Failed"),
                Total = g.Count()
            }).FirstOrDefaultAsync(cancellationToken);
        if (counts is null) return;
        batch.RefreshCounts(counts.Inserted, counts.Duplicates, counts.Failed, counts.Total);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static bool SamePatient(Guid? firstPatientId, string firstName, Guid? secondPatientId, string secondName) =>
        firstPatientId.HasValue && secondPatientId.HasValue
            ? firstPatientId.Value == secondPatientId.Value
            : NormalizePersonName(firstName) == NormalizePersonName(secondName);

    private static string NormalizePersonName(string value)
    {
        var trimmed = value.Trim().ToUpperInvariant();
        var comma = trimmed.IndexOf(',');
        if (comma > 0) trimmed = $"{trimmed[(comma + 1)..]} {trimmed[..comma]}";
        return string.Join(" ", trimmed.Replace(',', ' ')
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NormalizeToken(string? value) => string.Join("", (value ?? "").Trim().ToUpperInvariant().Where(c => !char.IsWhiteSpace(c)))
        .Replace('–', '-').Replace('—', '-');

    private static string NormalizeTimeBlock(string? value)
    {
        var parts = (value ?? string.Empty).Trim().Replace('–', '-').Replace('—', '-').Split('-', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && TryParseTime(parts[0], out var start) && TryParseTime(parts[1], out var end))
            return $"{(int)start.TotalMinutes:D4}|{(int)end.TotalMinutes:D4}";
        return NormalizeToken(value);
    }

    private static bool TryParseTime(string value, out TimeSpan time)
    {
        value = value.Trim().Replace(" ", string.Empty).ToUpperInvariant();
        if (DateTime.TryParseExact(value, new[] { "htt", "h:mmtt", "hh:mmtt", "H:mm", "HH:mm" },
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
        {
            time = parsed.TimeOfDay;
            return true;
        }
        time = default;
        return false;
    }
}
