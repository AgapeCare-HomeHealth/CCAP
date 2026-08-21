using CCAP.Application.Features.Dashboard.DTOs;
using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IAnnouncementRepository
{
    Task<List<AnnouncementDto>> GetActiveAsync(
        CancellationToken cancellationToken);

    Task<AnnouncementDto?> GetByIdAsync(
        Guid announcementId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Announcement announcement,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}