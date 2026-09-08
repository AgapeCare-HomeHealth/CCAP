namespace CCAP.Application.Features.Patients.DTOs;

public sealed class PatientCareProfileResponseDto
{
    public Guid PatientId { get; set; }

    public FaxInformationResponseDto Fax { get; set; } = new();

    public List<PatientNotificationResponseDto> Notifications { get; set; } = [];

    public List<PatientNoteResponseDto> Notes { get; set; } = [];

    public List<PatientServiceOrderResponseDto> ServiceOrders { get; set; } = [];

    public List<VisitResponseDto> UpcomingVisits { get; set; } = [];

    public List<PatientTaskResponseDto> Tasks { get; set; } = [];
}


// ============================================================
// FAX
// ============================================================

public sealed class FaxInformationResponseDto
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
// NOTIFICATIONS
// ============================================================
//
// No notification entity is currently available in the
// patient-care domain, so this DTO remains available for
// future implementation. Do not populate it with fake data.
//

public sealed class PatientNotificationResponseDto
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
// CALL NOTES
// ============================================================
//
// PatientNote in the Web UI is mapped from the real CallNote
// entity. We do NOT create fake patient notes.
//

public sealed class PatientNoteResponseDto
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
// PATIENT SERVICE ORDERS
// ============================================================

public sealed class PatientServiceOrderResponseDto
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
// VISITS
// ============================================================

public sealed class VisitResponseDto
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
// PATIENT TASKS
// ============================================================

public sealed class PatientTaskResponseDto
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