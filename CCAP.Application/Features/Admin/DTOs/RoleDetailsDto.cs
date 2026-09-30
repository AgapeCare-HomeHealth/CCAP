namespace CCAP.Application.Features.Admin.DTOs;

public sealed record RoleDetailsDto(
    Guid RoleId,
    string RoleName,
    string Description,
    bool IsActive,
    IReadOnlyList<PermissionDto> Permissions);
