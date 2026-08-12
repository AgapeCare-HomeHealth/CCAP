namespace CCAP.Domain.Entities;

public sealed class Activity
{
    private Activity() { }

    public Guid ActivityId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    public DateTime ActivityDate { get; private set; }
    public string ActivityType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Patient Patient { get; private set; } = null!;
    public ApplicationUser? PerformedBy { get; private set; }

    public Activity(Guid patientId, Guid? performedByUserId, string activityType, string title, string description)
    {
        ActivityId = Guid.NewGuid();
        PatientId = patientId;
        PerformedByUserId = performedByUserId;
        ActivityDate = DateTime.UtcNow;
        ActivityType = activityType;
        Title = title;
        Description = description;
    }
}
