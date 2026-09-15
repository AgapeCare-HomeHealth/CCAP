namespace CCAP.API.Contracts.Announcements;

public sealed class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
