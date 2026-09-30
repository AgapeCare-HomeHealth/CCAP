using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandValidator
    : IRequestValidator<CreateAnnouncementCommand>
{
    public void Validate(
        CreateAnnouncementCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException(
                "Announcement title is required.");
        }

        if (request.Title.Length > 200)
        {
            throw new ArgumentException(
                "Announcement title cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException(
                "Announcement message is required.");
        }

        if (request.Message.Length > 2000)
        {
            throw new ArgumentException(
                "Announcement message cannot exceed 2000 characters.");
        }

        if (request.ExpiresAt.HasValue &&
            request.ExpiresAt.Value <=
            (request.PublishedAt ?? DateTime.UtcNow))
        {
            throw new ArgumentException(
                "Expiration date must be after publication date.");
        }
    }
}