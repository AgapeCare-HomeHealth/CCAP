using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Role>> GetAllAsync(CancellationToken cancellationToken);
    Task<List<Permission>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<List<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
    Task ReplacePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken);
    Task AddAsync(Role role, CancellationToken cancellationToken);
    void Remove(Role role);
}
