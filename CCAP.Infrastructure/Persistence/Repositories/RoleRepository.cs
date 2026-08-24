using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;
    public RoleRepository(AppDbContext context) => _context = context;

    public Task<bool> ExistsByNameAsync(
    string roleName,
    Guid? excludeRoleId,
    CancellationToken cancellationToken)
    {
        var normalizedName =
            roleName.Trim().ToLower();

        return _context.Roles
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.RoleName.ToLower() == normalizedName &&
                    (!excludeRoleId.HasValue ||
                     x.RoleId != excludeRoleId.Value),
                cancellationToken);
    }

    public Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        _context.Roles
            .Include(x => x.Users)
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(
                x => x.RoleId == id,
                cancellationToken);

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
            var requestedIds =
                permissionIds
                    .Where(x => x != Guid.Empty)
                    .Distinct()
                    .ToList();

            // =========================================================
            // VALIDATE PERMISSION IDS
            // =========================================================

            if (requestedIds.Count > 0)
            {
                var existingPermissionIds =
                    await _context.Permissions
                        .Where(x =>
                            requestedIds.Contains(x.PermissionId))
                        .Select(x => x.PermissionId)
                        .ToListAsync(cancellationToken);

                var invalidPermissionIds =
                    requestedIds
                        .Except(existingPermissionIds)
                        .ToList();

                if (invalidPermissionIds.Count > 0)
                {
                    throw new InvalidOperationException(
                        "One or more selected permissions do not exist.");
                }
            }

            // =========================================================
            // REMOVE CURRENT PERMISSIONS
            // =========================================================

            var existing =
                await _context.RolePermissions
                    .Where(x => x.RoleId == roleId)
                    .ToListAsync(cancellationToken);

            _context.RolePermissions.RemoveRange(existing);

            // =========================================================
            // ADD NEW PERMISSIONS
            // =========================================================

            foreach (var permissionId in requestedIds)
            {
                _context.RolePermissions.Add(
                    new RolePermission(
                        roleId,
                        permissionId));
            }
        }

    public Task AddAsync(Role role, CancellationToken cancellationToken) =>
        _context.Roles.AddAsync(role, cancellationToken).AsTask();

    public void Update(Role role) => _context.Roles.Update(role);
    public void Remove(Role role) => _context.Roles.Remove(role);
}
