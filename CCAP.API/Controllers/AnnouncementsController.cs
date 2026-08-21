using System.Security.Claims;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/announcements")]
[Authorize]
public sealed class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementRepository _repository;

    public AnnouncementsController(
        IAnnouncementRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var announcements =
            await _repository.GetActiveAsync(
                cancellationToken);

        return Ok(announcements);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        Guid? userId =
            Guid.TryParse(userIdValue, out var parsed)
                ? parsed
                : null;

        var announcement = new Announcement(
            request.Title,
            request.Message,
            request.PublishedAt ?? DateTime.UtcNow,
            request.ExpiresAt,
            userId);

        await _repository.AddAsync(
            announcement,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Ok(new
        {
            announcementId =
                announcement.AnnouncementId
        });
    }
}

public sealed class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}