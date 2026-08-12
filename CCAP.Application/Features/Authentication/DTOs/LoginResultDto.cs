namespace CCAP.Application.Features.Authentication.DTOs;

public sealed record LoginResultDto(
    bool Success,
    string Message,
    string? Token,
    DateTime? Expiration,
    Guid? UserId,
    string? FullName,
    string? Role);
