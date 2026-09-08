namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;


// ============================================================
// FAX
// ============================================================

public sealed class FaxInformationDto
{
    public Guid FaxId { get; set; }

    public Guid PatientId { get; set; }

    public string FaxNumber { get; set; } = string.Empty;

    public string ReferringProvider { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public DateTime? ReceivedAt { get; set; }

    public bool Verified { get; set; }

    public string? Notes { get; set; }
}


// ============================================================
// NOTIFICATION
// ============================================================

public sealed class PatientNotificationDto
{
    public Guid NotificationId { get; set; }

    public Guid PatientId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsRead { get; set; }
}


// ============================================================
// PATIENT NOTE
// ============================================================

public sealed class PatientNoteDto
{
    public Guid NoteId { get; set; }

    public Guid PatientId { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Priority { get; set; } = "Normal";

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool Resolved { get; set; }

    public string? Outcome { get; set; }
}


// ============================================================
// SERVICE ORDER
// ============================================================

public sealed class PatientServiceOrderDto
{
    public Guid PatientServiceOrderId { get; set; }

    public Guid PatientId { get; set; }

    public Guid ServiceTypeId { get; set; }

    public string ServiceCode { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public string Duration { get; set; } = string.Empty;

    public bool IsPrimaryDiscipline { get; set; }
}


// ============================================================
// VISIT
// ============================================================

public sealed class VisitDto
{
    public Guid VisitId { get; set; }

    public Guid PatientId { get; set; }

    public DateTime ScheduledDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    public string? Notes { get; set; }
}


// ============================================================
// PATIENT TASK
// ============================================================

public sealed class PatientTaskDto
{
    public Guid TaskId { get; set; }

    public Guid PatientId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    public string? PageRoute { get; set; }

    public bool IsOverdue { get; set; }
}


// ============================================================
// PATIENT CARE PROFILE
// ============================================================

public sealed class PatientCareProfileDto
{
    public Guid PatientId { get; set; }

    public FaxInformationDto Fax { get; set; } = new();

    public List<PatientNotificationDto> Notifications { get; set; } = [];

    public List<PatientNoteDto> Notes { get; set; } = [];

    public List<PatientServiceOrderDto> ServiceOrders { get; set; } = [];

    public List<VisitDto> UpcomingVisits { get; set; } = [];

    public List<PatientTaskDto> Tasks { get; set; } = [];
}