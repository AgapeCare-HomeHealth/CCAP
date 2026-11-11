namespace CCAP.Web.Features.Dashboard.Models
{
    public class AnnouncementDto
    {
        public Guid AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
