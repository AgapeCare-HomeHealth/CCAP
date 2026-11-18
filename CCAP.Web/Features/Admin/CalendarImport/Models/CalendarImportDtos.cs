namespace CCAP.Web.Features.Admin.CalendarImport.Models;

public sealed class CalendarImportBatchDto
{
    public Guid CalendarImportBatchId { get; set; }
    public string FileName { get; set; } = "";
    public string Status { get; set; } = "";
    public int TotalRows { get; set; }
    public int InsertedRows { get; set; }
    public int DuplicateRows { get; set; }
    public int FailedRows { get; set; }
    public int WarningRows { get; set; }
    public string UploadedBy { get; set; } = "";
    public Guid? CareCoordinatorId { get; set; }
    public string CareCoordinatorName { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<CalendarImportRowDto> Rows { get; set; } = [];
}

public sealed class CalendarImportCareCoordinatorDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class CalendarImportRowDto
{
    public Guid CalendarImportRowId { get; set; }
    public int RowNumber { get; set; }
    public string SheetName { get; set; } = "CSV";
    public string PatientName { get; set; } = "";
    public DateTime? ScheduledDate { get; set; }
    public string TimeBlock { get; set; } = "";
    public string ConfirmationStatus { get; set; } = "";
    public string ClinicianName { get; set; } = "";
    public string VisitType { get; set; } = "";
    public string VisitStatus { get; set; } = "";
    public string? NotesFlag { get; set; }
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public bool HasWarning { get; set; }
}
