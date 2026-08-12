namespace CCAP.Application.Features.Admin.DTOs;

public sealed record PermissionDto(
    Guid PermissionId,
    string PermissionCode,
    string PermissionName,
    string Module,
    string Description);
