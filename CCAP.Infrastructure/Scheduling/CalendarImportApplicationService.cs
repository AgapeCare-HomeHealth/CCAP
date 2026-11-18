using System.Text;
using System.Text.Json;
using CCAP.Application.Abstractions.Scheduling;
using CCAP.Application.Features.Scheduling.Import;
using CCAP.Domain.Entities;
using CCAP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Scheduling;

public sealed class CalendarImportApplicationService : ICalendarImportApplicationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _db;
    private readonly IScheduleImportService _scheduleImport;

    public CalendarImportApplicationService(AppDbContext db, IScheduleImportService scheduleImport)
    {
        _db = db;
        _scheduleImport = scheduleImport;
    }

    public async Task<CalendarImportBatchDto> PreviewAsync(
        Guid uploadedByUserId,
        Guid careCoordinatorId,
        string fileName,
        byte[] fileBytes,
        CancellationToken cancellationToken)
    {
        if (fileBytes.Length == 0) throw new InvalidOperationException("Select a non-empty Excel or CSV file.");
        var careCoordinator = await _db.ApplicationUsers.Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == careCoordinatorId && x.IsActive && x.Role != null &&
                x.Role.RoleName == "Care Coordinator", cancellationToken);
        if (careCoordinator is null) throw new InvalidOperationException("Select an active Care Coordinator for this import.");
        await using var stream = new MemoryStream(fileBytes, writable: false);
        var previewRows = await _scheduleImport.PreviewForAdministratorAsync(uploadedByUserId, stream, fileName, cancellationToken);
        if (previewRows.Count == 0) throw new InvalidOperationException("The uploaded schedule does not contain any data rows.");

        var payloadRows = previewRows.Select(x => new CalendarImportRowDto
        {
            RowNumber = x.RowNumber,
            SheetName = x.SheetName,
            PatientName = x.PatientName,
            PatientId = x.PatientId,
            ScheduledDate = x.ScheduledDate == default ? null : x.ScheduledDate,
            TimeBlock = x.TimeBlock,
            ConfirmationStatus = x.ConfirmationStatus,
            CallNotes = x.CallNotes,
            ClinicianId = x.ClinicianId,
            ClinicianName = x.ClinicianName,
            VisitType = x.VisitType,
            VisitStatus = x.VisitStatus,
            NotesFlag = x.NotesFlag,
            Status = !x.CanImport ? (x.IsDuplicate ? "Duplicate" : "Failed") : "Ready",
            ErrorMessage = x.ValidationMessage,
            HasWarning = x.HasWarning
        }).ToList();

        var batch = new CalendarImportBatch(fileName, payloadRows.Count, payloadRows.Count(x => x.HasWarning), uploadedByUserId, careCoordinatorId);
        var rows = payloadRows.Select(x => new CalendarImportRow(batch.CalendarImportBatchId, x.RowNumber, x.SheetName,
            JsonSerializer.Serialize(x, JsonOptions), x.Status, x.ErrorMessage, x.HasWarning)).ToList();
        batch.RefreshCounts(0, rows.Count(x => x.Status == "Duplicate"), rows.Count(x => x.Status == "Failed"), rows.Count);
        _db.CalendarImportBatches.Add(batch);
        _db.CalendarImportRows.AddRange(rows);
        await _db.SaveChangesAsync(cancellationToken);

        for (var i = 0; i < payloadRows.Count; i++) payloadRows[i].CalendarImportRowId = rows[i].CalendarImportRowId;
        return Map(batch, await UserNameAsync(uploadedByUserId, cancellationToken), FullName(careCoordinator), payloadRows);
    }

    public async Task<CalendarImportBatchDto> QueueAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _db.CalendarImportBatches.FirstOrDefaultAsync(x => x.CalendarImportBatchId == batchId, cancellationToken)
            ?? throw new KeyNotFoundException("Calendar import preview was not found.");
        if (batch.Status != "Preview") throw new InvalidOperationException("This import has already been queued or processed.");
        if (!batch.CareCoordinatorId.HasValue) throw new InvalidOperationException("This preview has no assigned Care Coordinator. Upload the file again and choose a Care Coordinator.");
        if (!await _db.CalendarImportRows.AnyAsync(x => x.CalendarImportBatchId == batchId && x.Status == "Ready", cancellationToken))
            throw new InvalidOperationException("There are no valid schedule rows to import.");
        batch.Queue();
        await _db.SaveChangesAsync(cancellationToken);
        return await GetAsync(batchId, cancellationToken) ?? throw new KeyNotFoundException("Calendar import was not found.");
    }

    public async Task<CalendarImportBatchDto?> GetAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _db.CalendarImportBatches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CalendarImportBatchId == batchId, cancellationToken);
        if (batch is null) return null;
        var uploadedBy = await UserNameAsync(batch.UploadedByUserId, cancellationToken);
        var careCoordinator = await UserNameAsync(batch.CareCoordinatorId, cancellationToken);
        return Map(batch, uploadedBy, careCoordinator, []);
    }

    public async Task<IReadOnlyList<CalendarImportBatchDto>> HistoryAsync(CancellationToken cancellationToken)
    {
        var batches = await _db.CalendarImportBatches.AsNoTracking().OrderByDescending(x => x.UploadedAt).Take(100).ToListAsync(cancellationToken);
        var uploaderIds = batches.Select(x => x.UploadedByUserId).Distinct().ToArray();
        var users = await _db.ApplicationUsers.AsNoTracking().Where(x => uploaderIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.FirstName + " " + x.LastName, cancellationToken);
        var careCoordinatorIds = batches.Where(x => x.CareCoordinatorId.HasValue)
            .Select(x => x.CareCoordinatorId!.Value).Distinct().ToArray();
        var careCoordinators = await _db.ApplicationUsers.AsNoTracking().Where(x => careCoordinatorIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.FirstName + " " + x.LastName, cancellationToken);
        return batches.Select(x => Map(x, users.GetValueOrDefault(x.UploadedByUserId) ?? "Unknown",
            x.CareCoordinatorId.HasValue ? careCoordinators.GetValueOrDefault(x.CareCoordinatorId.Value) ?? "Unknown" : "Not assigned", [])).ToList();
    }

    public async Task<IReadOnlyList<CalendarImportRowDto>> LogsAsync(Guid batchId, CancellationToken cancellationToken)
    {
        if (!await _db.CalendarImportBatches.AnyAsync(x => x.CalendarImportBatchId == batchId, cancellationToken))
            throw new KeyNotFoundException("Calendar import was not found.");
        var rows = await _db.CalendarImportRows.AsNoTracking().Where(x => x.CalendarImportBatchId == batchId)
            .OrderBy(x => x.SheetName).ThenBy(x => x.RowNumber).ToListAsync(cancellationToken);
        return rows.Select(Deserialize).ToList();
    }

    public async Task<CalendarImportErrorFile?> DownloadErrorsAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var rows = await _db.CalendarImportRows.AsNoTracking()
            .Where(x => x.CalendarImportBatchId == batchId && (x.Status == "Failed" || x.Status == "Duplicate"))
            .OrderBy(x => x.SheetName).ThenBy(x => x.RowNumber).ToListAsync(cancellationToken);
        if (rows.Count == 0) return null;

        var batch = await _db.CalendarImportBatches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CalendarImportBatchId == batchId, cancellationToken);
        if (batch is null) return null;
        var careCoordinator = await UserNameAsync(batch.CareCoordinatorId, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", new[] { "Assigned Care Coordinator", "Sheet", "Row", "Patient Name", "Date", "Time Block", "Confirmation Status", "Call Notes", "Assigned Clinician", "Type of Visit", "Visit Status", "Notes / Flag", "Import Status", "Import Error" }.Select(CsvEscape)));
        foreach (var entity in rows)
        {
            var row = Deserialize(entity);
            builder.AppendLine(string.Join(",", new[]
            {
                careCoordinator, row.SheetName, row.RowNumber.ToString(), row.PatientName,
                row.ScheduledDate?.ToString("yyyy-MM-dd") ?? "", row.TimeBlock,
                row.ConfirmationStatus, row.CallNotes ?? "", row.ClinicianName, row.VisitType, row.VisitStatus,
                row.NotesFlag ?? "", entity.Status, entity.ErrorMessage ?? "Not imported"
            }.Select(CsvEscape)));
        }
        var name = $"CalendarImport_{batchId:N}_errors.csv";
        return new CalendarImportErrorFile(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), name);
    }

    private async Task<string> UserNameAsync(Guid? userId, CancellationToken cancellationToken) =>
        !userId.HasValue ? "Not assigned" : await _db.ApplicationUsers.AsNoTracking().Where(x => x.UserId == userId.Value)
            .Select(x => x.FirstName + " " + x.LastName).FirstOrDefaultAsync(cancellationToken) ?? "Unknown";

    private static string FullName(ApplicationUser user) => $"{user.FirstName} {user.LastName}".Trim();

    private static CalendarImportRowDto Deserialize(CalendarImportRow entity)
    {
        var row = JsonSerializer.Deserialize<CalendarImportRowDto>(entity.RawDataJson, JsonOptions) ?? new CalendarImportRowDto();
        row.CalendarImportRowId = entity.CalendarImportRowId;
        row.Status = entity.Status;
        row.ErrorMessage = entity.ErrorMessage;
        row.HasWarning = entity.HasWarning;
        return row;
    }

    private static CalendarImportBatchDto Map(CalendarImportBatch batch, string uploadedBy, string careCoordinatorName, List<CalendarImportRowDto> rows) => new()
    {
        CalendarImportBatchId = batch.CalendarImportBatchId,
        FileName = batch.FileName,
        Status = batch.Status,
        TotalRows = batch.TotalRows,
        InsertedRows = batch.InsertedRows,
        DuplicateRows = batch.DuplicateRows,
        FailedRows = batch.FailedRows,
        WarningRows = batch.WarningRows,
        UploadedBy = uploadedBy,
        CareCoordinatorId = batch.CareCoordinatorId,
        CareCoordinatorName = careCoordinatorName,
        UploadedAt = batch.UploadedAt,
        StartedAt = batch.StartedAt,
        CompletedAt = batch.CompletedAt,
        Rows = rows
    };

    private static string CsvEscape(string? value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}
