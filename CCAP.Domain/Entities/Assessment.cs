namespace CCAP.Domain.Entities;

public sealed class Assessment
{
    private Assessment() { }

    public Guid AssessmentId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid CompletedByUserId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? Notes { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public ApplicationUser CompletedBy { get; private set; } = null!;

    public Assessment(Guid patientId, Guid completedByUserId, string? notes)
    {
        AssessmentId = Guid.NewGuid();
        PatientId = patientId;
        CompletedByUserId = completedByUserId;
        CompletedAt = DateTime.UtcNow;
        Status = "Completed";
        Notes = notes;
    }
}
