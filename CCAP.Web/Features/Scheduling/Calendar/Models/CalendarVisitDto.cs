namespace CCAP.Web.Features.Scheduling.Calendar.Models;

public sealed class CalendarVisitDto
{
    public Guid VisitId { get; set; }
    public Guid? PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TimeBlock { get; set; }
    public string? ConfirmationStatus { get; set; }
    public string? CallNotes { get; set; }
    public string Clinician { get; set; } = string.Empty;
    public string? NotesFlag { get; set; }
    public string? Notes { get; set; }
}

public sealed class ScheduleImportPreviewItem
{
    public int RowNumber { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }

    public DateTime ScheduledDate { get; set; }
    public string TimeBlock { get; set; } = string.Empty;

    public string ConfirmationStatus { get; set; } = string.Empty;
    public string? CallNotes { get; set; }

    public Guid? ClinicianId { get; set; }
    public string ClinicianName { get; set; } = "Unassigned";

    public string VisitType { get; set; } = "Visit";
    public string VisitStatus { get; set; } = "Scheduled";
    public string? NotesFlag { get; set; }

    public bool IsDuplicate { get; set; }
    public Guid? ExistingVisitId { get; set; }
    public string? ExistingClinicianName { get; set; }
    public string? ExistingStatus { get; set; }

    public bool CanImport { get; set; } = true;
    public bool HasWarning { get; set; }
    public string? ValidationMessage { get; set; }
    public bool OverwriteExisting { get; set; }
}

public sealed class ScheduleImportCommitItem
{
    public string PatientName { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }

    public DateTime ScheduledDate { get; set; }
    public string TimeBlock { get; set; } = string.Empty;

    public string ConfirmationStatus { get; set; } = string.Empty;
    public string? CallNotes { get; set; }

    public Guid? ClinicianId { get; set; }
    public string? ClinicianName { get; set; }

    public string VisitType { get; set; } = "Visit";
    public string VisitStatus { get; set; } = "Scheduled";
    public string? NotesFlag { get; set; }

    public Guid? ExistingVisitId { get; set; }
    public bool HasWarning { get; set; }
    public bool OverwriteExisting { get; set; }
}

public sealed class ScheduleImportResult
{
    public int Added { get; set; }
    public int Overwritten { get; set; }
    public int Kept { get; set; }
    public int Skipped { get; set; }
    public int Warnings { get; set; }
}

public sealed class AddScheduleItem
{
    public DateTime? Date { get; set; }
    public string TimeBlock { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string ConfirmationStatus { get; set; } = string.Empty;
    public string? CallNotes { get; set; }
    public Guid? AssignedClinicianId { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string? VisitStatus { get; set; }
    public string? NotesFlag { get; set; }
}

public sealed class AddSchedulesResult
{
    public int Added { get; set; }
    public int SkippedDuplicates { get; set; }
    public IReadOnlyList<Guid> VisitIds { get; set; } = [];
}


public sealed class SchedulingOptionsDto
{
    public List<SchedulingClinicianDto> Clinicians { get; set; } = [];
    public List<SchedulingLookupOptionDto> ConfirmationStatuses { get; set; } = [];
    public List<SchedulingLookupOptionDto> VisitTypes { get; set; } = [];
    public List<SchedulingLookupOptionDto> VisitStatuses { get; set; } = [];
}

public sealed class SchedulingClinicianDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class SchedulingLookupOptionDto
{
    public Guid LookupOptionId { get; set; }
    public string LookupType { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
