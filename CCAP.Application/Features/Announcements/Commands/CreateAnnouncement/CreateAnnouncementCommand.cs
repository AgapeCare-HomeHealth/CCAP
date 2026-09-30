using CCAP.Application.Features.Announcements.DTOs;
using MediatR;

namespace CCAP.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(
    string Title,
    string Message,
    DateTime? PublishedAt,
    DateTime? ExpiresAt,
    Guid? CreatedByUserId)
    : IRequest<AnnouncementDto>;