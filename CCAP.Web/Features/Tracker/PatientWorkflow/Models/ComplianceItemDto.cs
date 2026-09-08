public class ComplianceItemDto
{
    public Guid ComplianceRecordId { get; set; }

    public string RequirementCode { get; set; } = string.Empty;

    public string RequirementName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public string CompletedByUserName { get; set; } = string.Empty;
}