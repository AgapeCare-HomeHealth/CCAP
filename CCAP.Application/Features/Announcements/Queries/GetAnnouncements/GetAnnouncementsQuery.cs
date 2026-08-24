using CCAP.Application.Features.Announcements.DTOs;
using MediatR;

namespace CCAP.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed record GetAnnouncementsQuery
    : IRequest<IReadOnlyList<AnnouncementDto>>;