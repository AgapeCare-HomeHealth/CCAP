namespace CCAP.Domain.Entities;

public sealed class CalendarImportRow
{
    private CalendarImportRow() { }

    public Guid CalendarImportRowId { get; private set; }
    public Guid CalendarImportBatchId { get; private set; }
    public int RowNumber { get; private set; }
    public string SheetName { get; private set; } = string.Empty;
    public string RawDataJson { get; private set; } = "{}";
    public string Status { get; private set; } = "Ready";
    public string? ErrorMessage { get; private set; }
    public bool HasWarning { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public CalendarImportRow(Guid batchId, int rowNumber, string sheetName, string rawDataJson, string status, string? errorMessage, bool hasWarning)
    {
        CalendarImportRowId = Guid.NewGuid();
        CalendarImportBatchId = batchId;
        RowNumber = rowNumber;
        SheetName = sheetName;
        RawDataJson = rawDataJson;
        Status = status;
        ErrorMessage = errorMessage;
        HasWarning = hasWarning;
    }

    public void MarkProcessing() { Status = "Processing"; ErrorMessage = null; }
    public void MarkInserted() { Status = "Inserted"; ErrorMessage = null; ProcessedAt = DateTime.UtcNow; }
    public void MarkDuplicate(string message) { Status = "Duplicate"; ErrorMessage = message; ProcessedAt = DateTime.UtcNow; }
    public void MarkFailed(string message) { Status = "Failed"; ErrorMessage = message.Length > 1900 ? message[..1900] : message; ProcessedAt = DateTime.UtcNow; }
}
