namespace CCAP.Application.Features.Admin.DTOs;

public sealed record RoleListItemDto(
    Guid RoleId,
    string RoleName,
    string Description,
    int UserCount,
    int PermissionCount,
    bool IsActive);
