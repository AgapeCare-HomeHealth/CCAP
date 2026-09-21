namespace CCAP.API.Contracts.Admin;

public sealed record CreateRoleRequest(string RoleName, string? Description, bool IsActive);
