using System.Security.Claims;
using CCAP.Application.Features.Announcements.Commands.CreateAnnouncement;
using CCAP.Application.Features.Announcements.Queries.GetAnnouncements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/announcements")]
[Authorize]
public sealed class AnnouncementsController
    : ControllerBase
{
    private readonly ISender _sender;

    public AnnouncementsController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetAnnouncementsQuery(),
                cancellationToken);

        return Ok(result);
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
            Guid.TryParse(
                userIdValue,
                out var parsed)
                ? parsed
                : null;

        var result =
            await _sender.Send(
                new CreateAnnouncementCommand(
                    request.Title,
                    request.Message,
                    request.PublishedAt,
                    request.ExpiresAt,
                    userId),
                cancellationToken);

        return Ok(result);
    }
}

public sealed class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}