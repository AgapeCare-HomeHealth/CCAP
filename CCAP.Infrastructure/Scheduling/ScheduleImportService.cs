using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Scheduling;
using CCAP.Application.Features.Scheduling.Import;
using CCAP.Domain.Entities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CCAP.Infrastructure.Scheduling;

public sealed class ScheduleImportService : IScheduleImportService
{
    private readonly IPatientRepository _patients;
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleImportService(IPatientRepository patients, IVisitRepository visits, IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _visits = visits;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ScheduleImportPreviewItem>> PreviewAsync(Guid userId, Stream fileStream, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);

        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Unsupported file type. Please upload an Excel (.xlsx) or CSV schedule file.");
        }

        List<ParsedRow> rows;

        try
        {
            rows = string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
                ? ReadCsvSchedule(fileStream)
                : ReadSchedule(fileStream);
        }
        catch (OpenXmlPackageException)
        {
            throw new InvalidOperationException(
                "The uploaded Excel file could not be read. Please make sure it is a valid .xlsx file.");
        }
        catch (InvalidDataException)
        {
            throw new InvalidOperationException(
                "The uploaded file is not a valid schedule file.");
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException(
                "The uploaded file has an invalid or unsupported data format.");
        }

        if (rows.Count == 0)
        {
            throw new InvalidOperationException(
                "The uploaded file does not contain any scheduled visits. " +
                "Make sure it follows the weekly schedule format with dates, time slots, and patient names.");
        }

        var patients = await _patients.GetAllAsync(cancellationToken);
        var result = new List<ScheduleImportPreviewItem>();

        foreach (var row in rows)
        {
            // Patient matching is optional. An imported schedule is an
            // external scheduling record and must be persisted even when the
            // patient has not yet been created or matched in CCAP.
            var patient = patients.FirstOrDefault(p =>
                NormalizeName($"{p.FirstName} {p.LastName}") == NormalizeName(row.PatientName) ||
                NormalizeName($"{p.LastName}, {p.FirstName}") == NormalizeName(row.PatientName));

            var item = new ScheduleImportPreviewItem
            {
                RowNumber = row.RowNumber,
                PatientName = row.PatientName,
                PatientId = patient?.PatientId,
                ClinicianId = patient?.ClinicianId,
                ClinicianName = patient?.Clinician is null
                    ? "Unassigned"
                    : $"{patient.Clinician.FirstName} {patient.Clinician.LastName}".Trim(),
                ScheduledDate = row.ScheduledDate,
                VisitType = row.VisitType,
                Location = row.Location,
                Notes = row.Notes,
                // Missing Patient/Clinician mappings are warnings, not
                // import blockers. The imported text is retained on Visit.
                CanImport = true
            };

            Visit? duplicate = null;

            if (patient is not null)
            {
                duplicate = await _visits.FindDuplicateAsync(
                    userId,
                    patient.PatientId,
                    row.ScheduledDate,
                    cancellationToken);

                // Also recognize a previously imported schedule that was
                // saved before the patient was matched in CCAP.
                duplicate ??= await _visits.FindImportedDuplicateAsync(
                    userId,
                    row.PatientName,
                    row.ScheduledDate,
                    cancellationToken);
            }
            else
            {
                duplicate = await _visits.FindImportedDuplicateAsync(
                    userId,
                    row.PatientName,
                    row.ScheduledDate,
                    cancellationToken);
            }

            if (duplicate is not null)
            {
                item.IsDuplicate = true;
                item.ExistingVisitId = duplicate.VisitId;
                item.ExistingStatus = duplicate.Status;
                item.ExistingClinicianName =
                    duplicate.ClinicianName ??
                    (duplicate.Clinician is null
                        ? "Unassigned"
                        : $"{duplicate.Clinician.FirstName} {duplicate.Clinician.LastName}".Trim());
            }

            if (patient is null)
            {
                item.HasWarning = true;
                item.ValidationMessage =
                    "Patient was not found in CCAP. The schedule will still be saved as an external schedule.";
            }
            else if (!patient.ClinicianId.HasValue)
            {
                item.HasWarning = true;
                item.ValidationMessage =
                    "Patient has no assigned clinician in CCAP. The schedule will still be saved as unassigned.";
            }

            result.Add(item);
        }

        return result;
    }

    public async Task<ScheduleImportResult> CommitAsync(Guid userId, IReadOnlyList<ScheduleImportCommitItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            throw new ArgumentException("No schedules were selected for import.");

        var added = 0;
        var overwritten = 0;
        var kept = 0;

        foreach (var item in items)
        {
            Visit? existing = null;

            if (item.PatientId.HasValue)
            {
                existing = await _visits.FindDuplicateAsync(
                    userId,
                    item.PatientId.Value,
                    item.ScheduledDate,
                    cancellationToken);

                existing ??= await _visits.FindImportedDuplicateAsync(
                    userId,
                    item.PatientName,
                    item.ScheduledDate,
                    cancellationToken);
            }
            else
            {
                existing = await _visits.FindImportedDuplicateAsync(
                    userId,
                    item.PatientName,
                    item.ScheduledDate,
                    cancellationToken);
            }

            if (existing is not null)
            {
                if (!item.OverwriteExisting)
                {
                    kept++;
                    continue;
                }

                existing.UpdateImportedSchedule(
                    item.PatientName,
                    item.ScheduledDate,
                    item.ClinicianName,
                    item.PatientId,
                    item.ClinicianId,
                    userId,
                    item.VisitType,
                    item.Location,
                    item.Notes);

                overwritten++;
                continue;
            }

            var importedVisit = Visit.CreateImportedSchedule(
                item.PatientName,
                item.ScheduledDate,
                item.ClinicianName,
                item.PatientId,
                item.ClinicianId,
                userId,
                item.VisitType,
                item.Location,
                item.Notes);

            await _visits.AddAsync(importedVisit, cancellationToken);
            added++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ScheduleImportResult
        {
            Added = added,
            Overwritten = overwritten,
            Kept = kept,
            Warnings = items.Count(x => x.HasWarning)
        };
    }

    private static string NormalizeName(string value) =>
        string.Join(" ", value.Trim().ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed record ParsedRow(int RowNumber, string PatientName, DateTime ScheduledDate, string VisitType, string? Location, string? Notes);

    private static List<ParsedRow> ReadCsvSchedule(Stream stream)
    {
        if (stream.CanSeek) stream.Position = 0;
        using var reader = new StreamReader(stream, leaveOpen: true);
        var lines = new List<string>();
        while (!reader.EndOfStream)
            lines.Add(reader.ReadLine() ?? string.Empty);

        var cells = lines.Select(ParseCsvLine).ToList();
        if (cells.Count == 0) return [];

        var dayGroups = new List<(int StartColumn, DateTime Date)>();
        foreach (var row in cells.Take(8))
        {
            for (var i = 0; i < row.Count; i++)
            {
                if (TryParseScheduleDate(null, row[i], out var date))
                    dayGroups.Add((i + 1, date.Date));
            }
        }

        dayGroups = dayGroups
            .GroupBy(x => new { x.StartColumn, Date = x.Date.Date })
            .Select(x => x.First())
            .ToList();

        if (dayGroups.Count == 0)
            throw new InvalidOperationException(
                "This file does not appear to be a weekly schedule. No schedule dates were found above the patient columns.");

        var hasTimeSlot = cells.Any(row =>
            row.Count > 0 && TryParseTimeRange(row[0], out _, out _));

        var headerText = string.Join(" ", cells.Take(8).SelectMany(x => x))
            .ToUpperInvariant();

        var hasPatientHeader = headerText.Contains("PATIENT");
        var hasScheduleLayoutHeader =
            headerText.Contains("CITY/ZIP") ||
            headerText.Contains("CITY / ZIP") ||
            headerText.Contains("ADD NOTES") ||
            headerText.Contains("NOTES");

        if (!hasTimeSlot || !hasPatientHeader || !hasScheduleLayoutHeader)
        {
            throw new InvalidOperationException(
                "The uploaded file is not in the expected weekly schedule format. " +
                "Expected dates, time slots, PATIENT, CITY/ZIP CODE, and ADD NOTES fields.");
        }

        var output = new List<ParsedRow>();
        for (var rowIndex = 0; rowIndex < cells.Count; rowIndex++)
        {
            var row = cells[rowIndex];
            var timeText = row.Count > 0 ? row[0] : null;
            if (!TryParseTimeRange(timeText, out var start, out _)) continue;
            foreach (var group in dayGroups)
            {
                var patient = GetCsvCell(row, group.StartColumn - 1)?.Trim();
                if (string.IsNullOrWhiteSpace(patient)) continue;
                var location = GetCsvCell(row, group.StartColumn)?.Trim();
                var notes = GetCsvCell(row, group.StartColumn + 1)?.Trim();
                var end = default(TimeSpan);
                TryParseTimeRange(timeText, out _, out end);
                output.Add(new ParsedRow(rowIndex + 1, patient, group.Date.Add(start), "Visit", location, notes));
            }
        }
        if (output.Count == 0)
            throw new InvalidOperationException(
                "The uploaded file has the weekly schedule structure, but no patient schedules were found.");

        return output;
    }

    private static string? GetCsvCell(List<string> row, int index) =>
        index >= 0 && index < row.Count ? row[index] : null;

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '\"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    current.Append('\"');
                    i++;
                }
                else quoted = !quoted;
            }
            else if (ch == ',' && !quoted)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else current.Append(ch);
        }
        values.Add(current.ToString());
        return values;
    }

    private static List<ParsedRow> ReadSchedule(Stream stream)
    {
        if (stream.CanSeek) stream.Position = 0;

        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("The workbook is invalid.");
        var sheet = workbookPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault();
        if (sheet is null)
            throw new InvalidOperationException("The uploaded Excel file does not contain a worksheet.");

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);
        var rows = worksheetPart.Worksheet.GetFirstChild<SheetData>()?.Elements<Row>().ToList() ?? [];
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

        // The real exported schedule is a single worksheet. Its layout follows
        // the same grid as the reference CLINICAL WORK SCHED worksheet:
        // time slots in column A, and each day occupying a group of columns
        // (PATIENT, CITY/ZIP CODE, ADD NOTES) with the date above the group.
        // Do not depend on a worksheet name.
        var dayGroups = new List<(int StartColumn, DateTime Date)>();

        foreach (var row in rows.Take(8))
        {
            foreach (var cell in row.Elements<Cell>())
            {
                var value = GetCellValue(cell, sharedStrings);
                if (TryParseScheduleDate(cell, value, out var date))
                {
                    var column = ColumnIndex(cell.CellReference!.Value!);
                    if (!dayGroups.Any(x => x.StartColumn == column && x.Date.Date == date.Date))
                        dayGroups.Add((column, date.Date));
                }
            }
        }

        if (dayGroups.Count == 0)
            throw new InvalidOperationException(
                "This file does not appear to be a weekly schedule. No schedule dates were found above the patient columns.");

        var headerText = string.Join(" ",
                rows.Take(8)
                    .SelectMany(r => r.Elements<Cell>())
                    .Select(c => GetCellValue(c, sharedStrings) ?? string.Empty))
            .ToUpperInvariant();

        var hasTimeSlot = rows.Any(r =>
        {
            var timeCell = r.Elements<Cell>().FirstOrDefault(c =>
                ColumnIndex(c.CellReference!.Value!) == 1);
            return TryParseTimeRange(GetCellValue(timeCell, sharedStrings), out _, out _);
        });

        var hasPatientHeader = headerText.Contains("PATIENT");
        var hasScheduleLayoutHeader =
            headerText.Contains("CITY/ZIP") ||
            headerText.Contains("CITY / ZIP") ||
            headerText.Contains("ADD NOTES") ||
            headerText.Contains("NOTES");

        if (!hasTimeSlot || !hasPatientHeader || !hasScheduleLayoutHeader)
        {
            throw new InvalidOperationException(
                "The uploaded file is not in the expected weekly schedule format. " +
                "Expected dates, time slots, PATIENT, CITY/ZIP CODE, and ADD NOTES fields.");
        }

        var output = new List<ParsedRow>();
        foreach (var row in rows)
        {
            var timeCell = row.Elements<Cell>().FirstOrDefault(c =>
                ColumnIndex(c.CellReference!.Value!) == 1);
            var timeText = GetCellValue(timeCell, sharedStrings);
            if (!TryParseTimeRange(timeText, out var start, out var end))
                continue;

            foreach (var group in dayGroups)
            {
                var patient = GetCellValue(GetCell(row, group.StartColumn), sharedStrings)?.Trim();
                if (string.IsNullOrWhiteSpace(patient))
                    continue;

                var location = GetCellValue(GetCell(row, group.StartColumn + 1), sharedStrings)?.Trim();
                var notes = GetCellValue(GetCell(row, group.StartColumn + 2), sharedStrings)?.Trim();
                output.Add(new ParsedRow(
                    (int)(row.RowIndex?.Value ?? 0),
                    patient,
                    group.Date.Add(start),
                    "Visit",
                    location,
                    notes));
            }
        }

        if (output.Count == 0)
            throw new InvalidOperationException(
                "The uploaded file has the weekly schedule structure, but no patient schedules were found.");

        return output;
    }

    private static Cell? GetCell(Row row, int column) => row.Elements<Cell>().FirstOrDefault(c => ColumnIndex(c.CellReference!.Value!) == column);

    private static string? GetCellValue(Cell? cell, SharedStringTable? sharedStrings)
    {
        if (cell is null) return null;
        var value = cell.CellValue?.InnerText ?? cell.InnerText;
        if (cell.DataType?.Value == CellValues.SharedString && int.TryParse(value, out var index))
            return sharedStrings?.ElementAtOrDefault(index)?.InnerText;
        if (cell.DataType?.Value == CellValues.InlineString) return cell.InlineString?.InnerText;
        return value;
    }

    private static bool TryParseScheduleDate(Cell? cell, string? value, out DateTime date)
    {
        date = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Excel stores real date cells as OLE Automation serial numbers.
        // Prefer that value because it is unambiguous regardless of how
        // Excel displays the date (MM/dd/yyyy, dd/MM/yyyy, etc.).
        if (double.TryParse(
                value.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var serial))
        {
            try
            {
                var candidate = DateTime.FromOADate(serial);

                if (candidate.Year >= 2000 && candidate.Year <= 2100)
                {
                    date = candidate.Date;
                    return true;
                }
            }
            catch (ArgumentException)
            {
                // Not a valid Excel/OLE date. Continue with text parsing.
            }
        }

        var text = value.Trim();

        // Try formats whose meaning is not dependent on the current
        // machine/browser culture.
        var explicitFormats = new[]
        {
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "yyyy.MM.dd",
            "MM/dd/yyyy",
            "M/d/yyyy",
            "MM-dd-yyyy",
            "M-d-yyyy",
            "MM.dd.yyyy",
            "M.d.yyyy",
            "MMM d, yyyy",
            "MMMM d, yyyy",
            "d MMM yyyy",
            "dd MMM yyyy",
            "d MMMM yyyy",
            "dd MMMM yyyy"
        };

        var candidates = new List<DateTime>();

        foreach (var format in explicitFormats)
        {
            if (DateTime.TryParseExact(
                    text,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var parsed)
                && parsed.Year >= 2000
                && parsed.Year <= 2100)
            {
                if (!candidates.Any(x => x.Date == parsed.Date))
                    candidates.Add(parsed.Date);
            }
        }

        if (candidates.Count == 1)
        {
            date = candidates[0];
            return true;
        }

        // A value such as 03/04/2026 is genuinely ambiguous:
        // it can mean March 4 or April 3. Never silently choose one.
        if (candidates.Count > 1)
        {
            throw new InvalidOperationException(
                $"The date '{text}' is ambiguous. Please use an unambiguous date " +
                "format such as YYYY-MM-DD, or make sure the Excel cell is stored as a date.");
        }

        // Last attempt for formats such as localized month names. This is
        // intentionally only accepted when the result is unambiguous.
        if (DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var fallback)
            && fallback.Year >= 2000
            && fallback.Year <= 2100)
        {
            date = fallback.Date;
            return true;
        }

        return false;
    }

    private static bool TryParseTimeRange(string? text, out TimeSpan start, out TimeSpan end)
    {
        start = end = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var parts = text.Trim().Replace("–", "-").Split('-', 2);
        if (parts.Length != 2) return false;
        return TryParseTime(parts[0], out start) && TryParseTime(parts[1], out end);
    }

    private static bool TryParseTime(string text, out TimeSpan time)
    {
        text = text.Trim().Replace(" ", "").ToUpperInvariant();
        if (DateTime.TryParseExact(text, new[] { "htt", "h:mmtt", "hh:mmtt", "H:mm", "HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        { time = parsed.TimeOfDay; return true; }
        time = default; return false;
    }

    private static int ColumnIndex(string reference)
    {
        var letters = new string(reference.TakeWhile(char.IsLetter).ToArray());
        var result = 0;
        foreach (var c in letters) result = result * 26 + (c - 'A' + 1);
        return result;
    }
}
