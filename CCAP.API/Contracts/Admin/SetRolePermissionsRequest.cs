namespace CCAP.API.Contracts.Admin;

public sealed record SetRolePermissionsRequest(IReadOnlyCollection<Guid> PermissionIds);
