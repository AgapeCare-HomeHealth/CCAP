using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class AdminLookupRepository : IAdminLookupRepository
{
    private readonly AppDbContext _context;
    public AdminLookupRepository(AppDbContext context) => _context = context;

    public Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken) =>
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

    public Task<List<Discipline>> GetDisciplinesAsync(CancellationToken cancellationToken) =>
        _context.Disciplines
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}
