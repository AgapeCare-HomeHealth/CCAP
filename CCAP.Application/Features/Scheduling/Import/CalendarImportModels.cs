namespace CCAP.Application.Features.Scheduling.Import;

public sealed class CalendarImportBatchDto
{
    public Guid CalendarImportBatchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int InsertedRows { get; set; }
    public int DuplicateRows { get; set; }
    public int FailedRows { get; set; }
    public int WarningRows { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public Guid? CareCoordinatorId { get; set; }
    public string CareCoordinatorName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<CalendarImportRowDto> Rows { get; set; } = [];
}

public sealed class CalendarImportRowDto
{
    public Guid CalendarImportRowId { get; set; }
    public int RowNumber { get; set; }
    public string SheetName { get; set; } = "CSV";
    public string PatientName { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string TimeBlock { get; set; } = string.Empty;
    public string ConfirmationStatus { get; set; } = string.Empty;
    public string? CallNotes { get; set; }
    public Guid? ClinicianId { get; set; }
    public string ClinicianName { get; set; } = string.Empty;
    public string VisitType { get; set; } = "Visit";
    public string VisitStatus { get; set; } = string.Empty;
    public string? NotesFlag { get; set; }
    public string Status { get; set; } = "Ready";
    public string? ErrorMessage { get; set; }
    public bool HasWarning { get; set; }
}

public sealed record CalendarImportErrorFile(byte[] Bytes, string FileName);
