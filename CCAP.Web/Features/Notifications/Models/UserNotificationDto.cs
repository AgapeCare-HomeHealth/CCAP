namespace CCAP.Web.Features.Notifications.Models;

public sealed class UserNotificationDto
{
    public Guid NotificationId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public DateTime? DueDate { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
