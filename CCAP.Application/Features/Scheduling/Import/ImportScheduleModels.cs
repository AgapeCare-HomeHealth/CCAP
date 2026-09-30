namespace CCAP.Application.Features.Scheduling.Import;

public sealed class ScheduleImportPreviewItem
{
    public int RowNumber { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
    public Guid? ClinicianId { get; set; }
    public string ClinicianName { get; set; } = "Unassigned";
    public DateTime ScheduledDate { get; set; }
    public string VisitType { get; set; } = "Visit";
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public bool IsDuplicate { get; set; }
    public Guid? ExistingVisitId { get; set; }
    public string? ExistingClinicianName { get; set; }
    public string? ExistingStatus { get; set; }
    public bool CanImport { get; set; } = true;
    public bool HasWarning { get; set; }
    public string? ValidationMessage { get; set; }
}

public sealed class ScheduleImportCommitItem
{
    public string PatientName { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
    public Guid? ExistingVisitId { get; set; }
    public Guid? ClinicianId { get; set; }
    public string? ClinicianName { get; set; }
    public bool HasWarning { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string VisitType { get; set; } = "Visit";
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public bool OverwriteExisting { get; set; }
}
