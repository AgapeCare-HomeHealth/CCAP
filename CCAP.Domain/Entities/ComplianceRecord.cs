namespace CCAP.Domain.Entities;

public sealed class ComplianceRecord
{
    private ComplianceRecord() { }

    public Guid ComplianceRecordId { get; private set; }
    public Guid PatientId { get; private set; }
    public string RequirementCode { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public string? ComplianceStatus { get; private set; }
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
        ComplianceStatus = "Yes";
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
    }

    public void RequireReverification()
    {
        ComplianceStatus = "No";
        IsCompleted = false;
        CompletedAt = null;
        CompletedByUserId = null;
    }

    public void SetStatus(string status, Guid userId)
    {
        var normalized = status?.Trim().ToUpperInvariant();
        if (normalized is not ("YES" or "NO" or "N/A"))
            throw new ArgumentException("Compliance status must be Yes, No, or N/A.", nameof(status));

        ComplianceStatus = normalized == "N/A" ? "N/A" : normalized == "YES" ? "Yes" : "No";
        IsCompleted = normalized is "YES" or "N/A";
        if (IsCompleted)
        {
            CompletedAt = DateTime.UtcNow;
            CompletedByUserId = userId;
        }
        else
        {
            CompletedAt = null;
            CompletedByUserId = null;
        }
    }

}
