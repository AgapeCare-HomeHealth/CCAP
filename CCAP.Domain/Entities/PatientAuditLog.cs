namespace CCAP.Domain.Entities;

public sealed class PatientAuditLog
{
    private PatientAuditLog() { }

    public Guid PatientAuditLogId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? Description { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public ApplicationUser? PerformedByUser { get; private set; }

    public PatientAuditLog(Guid patientId, Guid? performedByUserId, string entityType, string entityId,
        string action, string? oldValues, string? newValues, string? description)
    {
        PatientAuditLogId = Guid.NewGuid();
        PatientId = patientId;
        PerformedByUserId = performedByUserId;
        OccurredAt = DateTime.UtcNow;
        EntityType = entityType.Trim();
        EntityId = entityId.Trim();
        Action = action.Trim().ToUpperInvariant();
        OldValues = oldValues;
        NewValues = newValues;
        Description = description;
    }
}
