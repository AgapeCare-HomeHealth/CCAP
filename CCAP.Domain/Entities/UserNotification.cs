namespace CCAP.Domain.Entities;

public sealed class UserNotification
{
    private UserNotification() { }
    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Severity { get; private set; } = "Info";
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public UserNotification(Guid userId, Guid patientId, string patientName, string type,
        string title, string message, string severity = "Info", DateTime? dueDate = null)
    {
        NotificationId = Guid.NewGuid(); UserId = userId; PatientId = patientId;
        PatientName = patientName; Type = type; Title = title; Message = message;
        Severity = severity; DueDate = dueDate; CreatedAt = DateTime.UtcNow;
    }
}
