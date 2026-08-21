using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Dashboard.DTOs;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class AnnouncementRepository
    : IAnnouncementRepository
{
    private readonly AppDbContext _db;

    public AnnouncementRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AnnouncementDto>> GetActiveAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _db.Announcements
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.PublishedAt <= now &&
                (!x.ExpiresAt.HasValue ||
                 x.ExpiresAt > now))
            .OrderByDescending(x => x.PublishedAt)
            .Take(10)
            .Select(x => new AnnouncementDto
            {
                AnnouncementId = x.AnnouncementId,
                Title = x.Title,
                Message = x.Message,
                PublishedAt = x.PublishedAt,
                ExpiresAt = x.ExpiresAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AnnouncementDto?> GetByIdAsync(
        Guid announcementId,
        CancellationToken cancellationToken)
    {
        return await _db.Announcements
            .AsNoTracking()
            .Where(x => x.AnnouncementId == announcementId)
            .Select(x => new AnnouncementDto
            {
                AnnouncementId = x.AnnouncementId,
                Title = x.Title,
                Message = x.Message,
                PublishedAt = x.PublishedAt,
                ExpiresAt = x.ExpiresAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        Announcement announcement,
        CancellationToken cancellationToken)
    {
        await _db.Announcements.AddAsync(
            announcement,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}