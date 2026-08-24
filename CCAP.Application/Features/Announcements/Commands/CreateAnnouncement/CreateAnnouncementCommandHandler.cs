using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Announcements.DTOs;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler
    : IRequestHandler<
        CreateAnnouncementCommand,
        AnnouncementDto>
{
    private readonly IAnnouncementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAnnouncementCommandHandler(
        IAnnouncementRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AnnouncementDto> Handle(
        CreateAnnouncementCommand request,
        CancellationToken cancellationToken)
    {
        var publishedAt =
            request.PublishedAt ??
            DateTime.UtcNow;

        var announcement =
            new Announcement(
                request.Title,
                request.Message,
                publishedAt,
                request.ExpiresAt,
                request.CreatedByUserId);

        await _repository.AddAsync(
            announcement,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new AnnouncementDto(
            announcement.AnnouncementId,
            announcement.Title,
            announcement.Message,
            announcement.PublishedAt,
            announcement.ExpiresAt);
    }
}