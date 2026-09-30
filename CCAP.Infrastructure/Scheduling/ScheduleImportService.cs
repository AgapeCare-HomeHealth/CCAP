using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Scheduling;
using CCAP.Application.Features.Scheduling.Import;
using CCAP.Domain.Entities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace CCAP.Infrastructure.Scheduling;

public sealed class ScheduleImportService : IScheduleImportService
{
    private static readonly HashSet<string> ClinicianCredentials =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "RN", "LVN", "HHA", "PT", "OT", "ST", "MSW", "LCSW", "NP", "MD"
        };

    private readonly IPatientRepository _patients;
    private readonly IUserRepository _users;
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleImportService(
        IPatientRepository patients,
        IUserRepository users,
        IVisitRepository visits,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _users = users;
        _visits = visits;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ScheduleImportPreviewItem>> PreviewAsync(
        Guid userId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken)
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
                : ReadExcelSchedule(fileStream);
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
                "The uploaded schedule does not contain any valid rows.");
        }

        var patients = await _patients.GetAllAsync(cancellationToken);
        var users = await _users.GetAllAsync(cancellationToken);

        var clinicians = users
            .Where(x =>
                x.IsActive &&
                x.Role is not null &&
                string.Equals(
                    x.Role.RoleName,
                    "Clinician",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        var result = new List<ScheduleImportPreviewItem>(rows.Count);

        foreach (var row in rows)
        {
            var patient = FindPatient(patients, row.PatientName);
            var clinician = FindClinician(clinicians, row.AssignedClinician);

            var item = new ScheduleImportPreviewItem
            {
                RowNumber = row.RowNumber,
                PatientName = row.PatientName,
                PatientId = patient?.PatientId,
                ScheduledDate = row.ScheduledDate,
                TimeBlock = row.TimeBlock,
                ConfirmationStatus = row.ConfirmationStatus,
                CallNotes = row.CallNotes,
                ClinicianId = clinician?.UserId,
                ClinicianName = row.AssignedClinician,
                VisitType = row.VisitType,
                VisitStatus = row.VisitStatus,
                NotesFlag = row.NotesFlag,
                CanImport = true
            };

            var warnings = new List<string>();

            if (patient is null)
                warnings.Add("Patient was not found in CCAP. The original patient name will be stored on the schedule.");

            if (string.IsNullOrWhiteSpace(row.AssignedClinician))
                warnings.Add("Assigned clinician is blank.");
            else if (clinician is null)
                warnings.Add(
                    $"Assigned clinician '{row.AssignedClinician}' was not matched to an active CCAP clinician. The original name will be retained.");

            if (string.IsNullOrWhiteSpace(row.ConfirmationStatus))
                warnings.Add("Confirmation Status is blank.");

            if (string.IsNullOrWhiteSpace(row.VisitType))
                warnings.Add("Type of Visit is blank.");

            if (string.IsNullOrWhiteSpace(row.VisitStatus))
                warnings.Add("Visit Status is blank. The schedule will use Scheduled.");

            if (warnings.Count > 0)
            {
                item.HasWarning = true;
                item.ValidationMessage = string.Join(" ", warnings);
            }

            var duplicate = await _visits.FindScheduleDuplicateAsync(
                userId,
                row.PatientName,
                row.ScheduledDate,
                row.TimeBlock,
                row.VisitType,
                cancellationToken);

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

            result.Add(item);
        }

        return result;
    }

    public async Task<ScheduleImportResult> CommitAsync(
        Guid userId,
        IReadOnlyList<ScheduleImportCommitItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            throw new ArgumentException("No schedules were selected for import.");

        var added = 0;
        var overwritten = 0;
        var kept = 0;

        foreach (var item in items)
        {
            var existing = await _visits.FindScheduleDuplicateAsync(
                userId,
                item.PatientName,
                item.ScheduledDate,
                item.TimeBlock,
                item.VisitType,
                cancellationToken);

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
                    item.TimeBlock,
                    item.ConfirmationStatus,
                    item.CallNotes,
                    item.ClinicianName,
                    item.PatientId,
                    item.ClinicianId,
                    userId,
                    item.VisitType,
                    item.VisitStatus,
                    item.NotesFlag);

                overwritten++;
                continue;
            }

            var importedVisit = Visit.CreateImportedSchedule(
                item.PatientName,
                item.ScheduledDate,
                item.TimeBlock,
                item.ConfirmationStatus,
                item.CallNotes,
                item.ClinicianName,
                item.PatientId,
                item.ClinicianId,
                userId,
                item.VisitType,
                item.VisitStatus,
                item.NotesFlag);

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

    private static Patient? FindPatient(
        IEnumerable<Patient> patients,
        string patientName)
    {
        var normalized = NormalizePersonName(patientName);

        return patients.FirstOrDefault(p =>
            NormalizePersonName($"{p.FirstName} {p.LastName}") == normalized ||
            NormalizePersonName($"{p.LastName}, {p.FirstName}") == normalized);
    }

    private static ApplicationUser? FindClinician(
        IReadOnlyList<ApplicationUser> clinicians,
        string? sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
            return null;

        var normalizedSource = NormalizeClinicianName(sourceName);

        // First try the complete name.
        var exact = clinicians
            .Where(x =>
                NormalizePersonName($"{x.FirstName} {x.LastName}") == normalizedSource ||
                NormalizePersonName($"{x.LastName}, {x.FirstName}") == normalizedSource)
            .ToList();

        if (exact.Count == 1)
            return exact[0];

        // The supplied template commonly contains first name + credential,
        // e.g. "NICOLE LVN" or "MARK RN". If the first name is unique,
        // use it as a safe fallback.
        var firstName = normalizedSource.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            var firstNameMatches = clinicians
                .Where(x => NormalizePersonName(x.FirstName) == firstName)
                .ToList();

            if (firstNameMatches.Count == 1)
                return firstNameMatches[0];
        }

        return null;
    }

    private static string NormalizeClinicianName(string value)
    {
        var tokens = value
            .Trim()
            .ToUpperInvariant()
            .Replace(",", " ")
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !ClinicianCredentials.Contains(x))
            .ToArray();

        return NormalizePersonName(string.Join(" ", tokens));
    }

    private static string NormalizePersonName(string value) =>
        string.Join(
            " ",
            value
                .Trim()
                .ToUpperInvariant()
                .Replace(",", " ")
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed record ParsedRow(
        int RowNumber,
        string PatientName,
        DateTime ScheduledDate,
        string TimeBlock,
        string ConfirmationStatus,
        string? CallNotes,
        string AssignedClinician,
        string VisitType,
        string VisitStatus,
        string? NotesFlag);

    private static List<ParsedRow> ReadExcelSchedule(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        using var document = SpreadsheetDocument.Open(stream, false);

        var workbookPart = document.WorkbookPart
            ?? throw new InvalidOperationException("The workbook is invalid.");

        var sharedStrings =
            workbookPart.SharedStringTablePart?.SharedStringTable;

        var output = new List<ParsedRow>();
        var foundTemplateSheet = false;

        foreach (var sheet in workbookPart.Workbook.Sheets?.Elements<Sheet>() ?? [])
        {
            var worksheetPart =
                (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);

            var rows = worksheetPart.Worksheet
                .GetFirstChild<SheetData>()?
                .Elements<Row>()
                .ToList() ?? [];

            if (rows.Count == 0)
                continue;

            var headerRow = rows.FirstOrDefault(r =>
                r.Elements<Cell>().Any(c =>
                    string.Equals(
                        GetCellValue(c, sharedStrings)?.Trim(),
                        "Patient Name",
                        StringComparison.OrdinalIgnoreCase)));

            // Ignore worksheets that are not schedule data sheets.
            // This allows the workbook to contain a blank "Template" sheet
            // alongside one or more populated schedule sheets.
            if (headerRow is null)
                continue;

            foundTemplateSheet = true;

            var columns = BuildHeaderMap(headerRow, sharedStrings);
            ValidateRequiredHeaders(columns);

            foreach (var row in rows)
            {
                if (ReferenceEquals(row, headerRow))
                    continue;

                var patientName = GetMappedCell(
                    row, columns, "Patient Name", sharedStrings)?.Trim();

                if (string.IsNullOrWhiteSpace(patientName))
                    continue;

                var rowNumber = (int)(row.RowIndex?.Value ?? 0);

                var dateText = GetMappedCell(
                    row, columns, "Date", sharedStrings);

                if (!TryParseScheduleDate(dateText, out var date))
                    throw new InvalidOperationException(
                        $"Sheet '{sheet.Name?.Value}' row {rowNumber}: Date is missing or invalid.");

                var timeBlock = GetMappedCell(
                    row, columns, "Time Block", sharedStrings)?.Trim();

                if (string.IsNullOrWhiteSpace(timeBlock))
                    throw new InvalidOperationException(
                        $"Sheet '{sheet.Name?.Value}' row {rowNumber}: Time Block is required.");

                ValidateTimeBlock(timeBlock);

                var confirmationStatus = GetMappedCell(
                    row, columns, "Confirmation Status", sharedStrings)?.Trim()
                    ?? string.Empty;

                var callNotes = GetMappedCell(
                    row, columns, "Call Notes", sharedStrings)?.Trim();

                var assignedClinician = GetMappedCell(
                    row, columns, "Assigned Clinician", sharedStrings)?.Trim()
                    ?? string.Empty;

                var visitType = GetMappedCell(
                    row, columns, "Type of Visit", sharedStrings)?.Trim();

                var visitStatus = GetMappedCell(
                    row, columns, "Visit Status", sharedStrings)?.Trim();

                var notesFlag = GetMappedCell(
                    row, columns, "Notes / Flag", sharedStrings)?.Trim();

                output.Add(new ParsedRow(
                    rowNumber,
                    patientName,
                    date.Date,
                    timeBlock,
                    confirmationStatus,
                    NullIfEmpty(callNotes),
                    assignedClinician,
                    string.IsNullOrWhiteSpace(visitType) ? "Visit" : visitType,
                    string.IsNullOrWhiteSpace(visitStatus) ? "Scheduled" : visitStatus,
                    NullIfEmpty(notesFlag)));
            }
        }

        if (!foundTemplateSheet)
            throw new InvalidOperationException(
                "The uploaded Excel file does not match the CC Sched template. The required schedule headers were not found.");

        return output;
    }

    private static List<ParsedRow> ReadCsvSchedule(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        using var reader = new StreamReader(stream, leaveOpen: true);

        var rows = new List<List<string>>();

        while (!reader.EndOfStream)
            rows.Add(ParseCsvLine(reader.ReadLine() ?? string.Empty));

        if (rows.Count == 0)
            return [];

        var headerIndex = rows.FindIndex(row =>
            row.Any(cell =>
                string.Equals(
                    cell.Trim(),
                    "Patient Name",
                    StringComparison.OrdinalIgnoreCase)));

        if (headerIndex < 0)
            throw new InvalidOperationException(
                "The uploaded CSV file does not match the CC Sched template. The 'Patient Name' header was not found.");

        var columns = BuildHeaderMap(rows[headerIndex]);
        ValidateRequiredHeaders(columns);

        var output = new List<ParsedRow>();

        for (var i = headerIndex + 1; i < rows.Count; i++)
        {
            var row = rows[i];

            var patientName = GetMappedCell(
                row, columns, "Patient Name")?.Trim();

            if (string.IsNullOrWhiteSpace(patientName))
                continue;

            var dateText = GetMappedCell(row, columns, "Date");

            if (!TryParseScheduleDate(dateText, out var date))
                throw new InvalidOperationException(
                    $"Row {i + 1}: Date is missing or invalid.");

            var timeBlock = GetMappedCell(row, columns, "Time Block")?.Trim();

            if (string.IsNullOrWhiteSpace(timeBlock))
                throw new InvalidOperationException(
                    $"Row {i + 1}: Time Block is required.");

            ValidateTimeBlock(timeBlock);

            var confirmationStatus =
                GetMappedCell(row, columns, "Confirmation Status")?.Trim() ?? string.Empty;

            var callNotes =
                GetMappedCell(row, columns, "Call Notes")?.Trim();

            var assignedClinician =
                GetMappedCell(row, columns, "Assigned Clinician")?.Trim() ?? string.Empty;

            var visitType =
                GetMappedCell(row, columns, "Type of Visit")?.Trim();

            var visitStatus =
                GetMappedCell(row, columns, "Visit Status")?.Trim();

            var notesFlag =
                GetMappedCell(row, columns, "Notes / Flag")?.Trim();

            output.Add(new ParsedRow(
                i + 1,
                patientName,
                date.Date,
                timeBlock,
                confirmationStatus,
                NullIfEmpty(callNotes),
                assignedClinician,
                string.IsNullOrWhiteSpace(visitType) ? "Visit" : visitType,
                string.IsNullOrWhiteSpace(visitStatus) ? "Scheduled" : visitStatus,
                NullIfEmpty(notesFlag)));
        }

        return output;
    }

    private static Dictionary<string, int> BuildHeaderMap(
        Row headerRow,
        SharedStringTable? sharedStrings)
    {
        var result = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.Elements<Cell>())
        {
            var header = NormalizeHeader(
                GetCellValue(cell, sharedStrings));

            if (string.IsNullOrWhiteSpace(header))
                continue;

            result[header] =
                ColumnIndex(cell.CellReference?.Value ?? string.Empty);
        }

        return result;
    }

    private static Dictionary<string, int> BuildHeaderMap(
        IReadOnlyList<string> headerRow)
    {
        var result = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < headerRow.Count; i++)
        {
            var header = NormalizeHeader(headerRow[i]);

            if (!string.IsNullOrWhiteSpace(header))
                result[header] = i;
        }

        return result;
    }

    private static void ValidateRequiredHeaders(
        IReadOnlyDictionary<string, int> columns)
    {
        var required = new[]
        {
            "Date",
            "Time Block",
            "Patient Name",
            "Confirmation Status",
            "Assigned Clinician",
            "Type of Visit"
        };

        var missing = required
            .Where(x => !columns.ContainsKey(NormalizeHeader(x)))
            .ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                "The uploaded file does not match the CC Sched template. " +
                $"Missing column(s): {string.Join(", ", missing)}.");
        }
    }

    private static string NormalizeHeader(string? value) =>
        string.Join(
            " ",
            (value ?? string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string? GetMappedCell(
        Row row,
        IReadOnlyDictionary<string, int> columns,
        string header,
        SharedStringTable? sharedStrings)
    {
        return columns.TryGetValue(NormalizeHeader(header), out var column)
            ? GetCellValue(
                GetCell(row, column),
                sharedStrings)
            : null;
    }

    private static string? GetMappedCell(
        IReadOnlyList<string> row,
        IReadOnlyDictionary<string, int> columns,
        string header)
    {
        return columns.TryGetValue(NormalizeHeader(header), out var column) &&
               column >= 0 &&
               column < row.Count
            ? row[column]
            : null;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Cell? GetCell(Row row, int column) =>
        row.Elements<Cell>()
            .FirstOrDefault(c =>
                ColumnIndex(c.CellReference?.Value ?? string.Empty) == column);

    private static string? GetCellValue(
        Cell? cell,
        SharedStringTable? sharedStrings)
    {
        if (cell is null)
            return null;

        var value = cell.CellValue?.InnerText ?? cell.InnerText;

        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(value, out var index))
        {
            return sharedStrings?.ElementAtOrDefault(index)?.InnerText;
        }

        if (cell.DataType?.Value == CellValues.InlineString)
            return cell.InlineString?.InnerText;

        return value;
    }

    private static bool TryParseScheduleDate(
        string? value,
        out DateTime date)
    {
        date = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();

        if (double.TryParse(
                text,
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
                // Continue with explicit text formats.
            }
        }

        var formats = new[]
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

        var candidates = formats
            .Select(format =>
            {
                return DateTime.TryParseExact(
                    text,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var parsed)
                    ? parsed.Date
                    : (DateTime?)null;
            })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Where(x => x.Year >= 2000 && x.Year <= 2100)
            .Distinct()
            .ToList();

        if (candidates.Count == 1)
        {
            date = candidates[0];
            return true;
        }

        if (candidates.Count > 1)
        {
            throw new InvalidOperationException(
                $"The date '{text}' is ambiguous. Please use an unambiguous date format.");
        }

        return false;
    }

    private static void ValidateTimeBlock(string timeBlock)
    {
        var normalized = timeBlock.Trim().Replace('–', '-');
        var parts = normalized.Split('-', 2);

        if (parts.Length != 2 ||
            !TryParseTime(parts[0], out _) ||
            !TryParseTime(parts[1], out _))
        {
            throw new InvalidOperationException(
                $"Invalid Time Block '{timeBlock}'. Expected a range such as '10:00AM–12:00PM'.");
        }
    }

    private static bool TryParseTime(
        string value,
        out TimeSpan time)
    {
        value = value.Trim().Replace(" ", string.Empty).ToUpperInvariant();

        if (DateTime.TryParseExact(
                value,
                new[] { "htt", "h:mmtt", "hh:mmtt", "H:mm", "HH:mm" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            time = parsed.TimeOfDay;
            return true;
        }

        time = default;
        return false;
    }

    private static int ColumnIndex(string reference)
    {
        var letters = new string(
            reference.TakeWhile(char.IsLetter).ToArray());

        var result = 0;

        foreach (var c in letters)
            result = result * 26 + (c - 'A' + 1);

        return result;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var quoted = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (quoted &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (ch == ',' && !quoted)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        values.Add(current.ToString());
        return values;
    }
}
