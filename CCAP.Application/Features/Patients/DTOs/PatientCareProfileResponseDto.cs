namespace CCAP.Application.Features.Patients.DTOs;

public sealed class PatientCareProfileResponseDto
{
    public Guid PatientId { get; set; }

    public List<PatientNotificationResponseDto> Notifications { get; set; } = [];

    public List<PatientNoteResponseDto> Notes { get; set; } = [];

    public List<LabOrderResponseDto> LabOrders { get; set; } = [];

    public List<WoundSupplyResponseDto> WoundSupplies { get; set; } = [];

    public List<FoleyChangeResponseDto> FoleyChanges { get; set; } = [];

    public List<OrderAlertResponseDto> OrderAlerts { get; set; } = [];

    public FaxInformationResponseDto Fax { get; set; } = new();
}

public sealed class FaxInformationResponseDto
{
    public Guid FaxId { get; set; }
    public Guid PatientId { get; set; }
    public string FaxNumber { get; set; } = string.Empty;
    public string ReferringProvider { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool Verified { get; set; }
    public string? Notes { get; set; }
}

public sealed class PatientNotificationResponseDto
{
    public Guid NotificationId { get; set; }
    public Guid PatientId { get; set; }
    public string Type { get; set; } = "Reminder";
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRead { get; set; }
}

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
}

public sealed class LabOrderResponseDto
{
    public Guid LabOrderId { get; set; }
    public Guid PatientId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string OrderingProvider { get; set; } = string.Empty;
    public DateTime OrderedDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Ordered";
    public string? Notes { get; set; }
}

public sealed class WoundSupplyResponseDto
{
    public Guid SupplyId { get; set; }
    public Guid PatientId { get; set; }
    public string SupplyName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string Status { get; set; } = "Required";
    public DateTime? NeededBy { get; set; }
    public string? Notes { get; set; }
}

public sealed class FoleyChangeResponseDto
{
    public Guid FoleyChangeId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime ChangeDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string CatheterSize { get; set; } = string.Empty;
    public string BalloonSize { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public sealed class OrderAlertResponseDto
{
    public Guid OrderAlertId { get; set; }
    public Guid PatientId { get; set; }
    public string OrderType { get; set; } = "POC";
    public DateTime OrderDate { get; set; }
    public DateTime? SignatureDue30Date { get; set; }
    public DateTime? SignatureDue60Date { get; set; }
    public bool Signed { get; set; }
    public string Status { get; set; } = "Pending PCP Signature";
    public string? Notes { get; set; }
}