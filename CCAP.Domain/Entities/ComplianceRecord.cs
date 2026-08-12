namespace CCAP.Domain.Entities;

public sealed class ComplianceRecord
{
    private ComplianceRecord() { }

    public Guid ComplianceRecordId { get; private set; }
    public Guid PatientId { get; private set; }
    public string RequirementCode { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public string? Notes { get; private set; }

    public Patient Patient { get; private set; } = null!;

    public ComplianceRecord(Guid patientId, string requirementCode, string? notes)
    {
        ComplianceRecordId = Guid.NewGuid();
        PatientId = patientId;
        RequirementCode = requirementCode.Trim();
        Notes = notes;
    }

    public void Complete(Guid userId)
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
    }
}
