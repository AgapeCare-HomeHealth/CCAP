using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;
    public RoleRepository(AppDbContext context) => _context = context;

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Roles
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.RoleId == id, cancellationToken);

    public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken) =>
        _context.Roles
            .Include(x => x.Users)
            .Include(x => x.RolePermissions)
            .AsNoTracking()
            .OrderBy(x => x.RoleName)
            .ToListAsync(cancellationToken);

    public Task<List<Permission>> GetPermissionsAsync(CancellationToken cancellationToken) =>
        _context.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Module)
            .ThenBy(x => x.PermissionName)
            .ToListAsync(cancellationToken);

    public Task<List<Permission>> GetPermissionsByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken) =>
        _context.RolePermissions
            .Where(x => x.RoleId == roleId)
            .Select(x => x.Permission)
            .AsNoTracking()
            .OrderBy(x => x.Module)
            .ThenBy(x => x.PermissionName)
            .ToListAsync(cancellationToken);

    public async Task ReplacePermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var existing = await _context.RolePermissions
            .Where(x => x.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(existing);

        var validIds = await _context.Permissions
            .Where(x => permissionIds.Contains(x.PermissionId))
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permissionId in validIds.Distinct())
            _context.RolePermissions.Add(new RolePermission(roleId, permissionId));
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken) =>
        _context.Roles.AddAsync(role, cancellationToken).AsTask();
}
