namespace CCAP.API.Contracts.Admin;

public sealed record UpdateRoleRequest(string RoleName, string? Description, bool IsActive);
