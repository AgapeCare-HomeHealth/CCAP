using System;
using System.Collections.Generic;
using System.Text;

namespace CCAP.Application.Features.Announcements.DTOs
{
    public sealed record AnnouncementDto(
        Guid AnnouncementId,
        string Title,
        string Message,
        DateTime PublishedAt,
        DateTime? ExpiresAt);
}
