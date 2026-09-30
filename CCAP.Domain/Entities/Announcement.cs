namespace CCAP.Domain.Entities;

public sealed class Announcement
{
    private Announcement()
    {
    }

    public Guid AnnouncementId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public DateTime PublishedAt { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    public bool IsActive { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public ApplicationUser? CreatedByUser { get; private set; }

    public Announcement(
        string title,
        string message,
        DateTime publishedAt,
        DateTime? expiresAt,
        Guid? createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Announcement title is required.",
                nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(
                "Announcement message is required.",
                nameof(message));

        if (expiresAt.HasValue &&
            expiresAt.Value <= publishedAt)
        {
            throw new ArgumentException(
                "Expiration date must be after publication date.",
                nameof(expiresAt));
        }

        AnnouncementId = Guid.NewGuid();
        Title = title.Trim();
        Message = message.Trim();
        PublishedAt = publishedAt;
        ExpiresAt = expiresAt;
        CreatedByUserId = createdByUserId;
        IsActive = true;
    }

    public void Update(
        string title,
        string message,
        DateTime publishedAt,
        DateTime? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Announcement title is required.",
                nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(
                "Announcement message is required.",
                nameof(message));

        if (expiresAt.HasValue &&
            expiresAt.Value <= publishedAt)
        {
            throw new ArgumentException(
                "Expiration date must be after publication date.",
                nameof(expiresAt));
        }

        Title = title.Trim();
        Message = message.Trim();
        PublishedAt = publishedAt;
        ExpiresAt = expiresAt;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}