namespace CCAP.Application.Features.Notifications.DTOs;

public sealed record NotificationDto(
    Guid NotificationId,
    Guid PatientId,
    string PatientName,
    string Type,
    string Title,
    string Message,
    string Severity,
    DateTime? DueDate,
    DateTime CreatedAt);