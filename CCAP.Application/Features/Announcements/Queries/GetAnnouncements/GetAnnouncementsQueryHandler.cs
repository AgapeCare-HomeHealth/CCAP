using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Announcements.DTOs;
using MediatR;

namespace CCAP.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed class GetAnnouncementsQueryHandler
    : IRequestHandler<
        GetAnnouncementsQuery,
        IReadOnlyList<AnnouncementDto>>
{
    private readonly IAnnouncementRepository _repository;

    public GetAnnouncementsQueryHandler(
        IAnnouncementRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AnnouncementDto>> Handle(
        GetAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetActiveAsync(
            cancellationToken);
    }
}