using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ApplicationUser?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        _context.ApplicationUsers
            .Include(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
            .Include(x => x.Discipline)
            .FirstOrDefaultAsync(
                x => x.UserId == id,
                cancellationToken);

    public async Task<List<ApplicationUser>> GetByIdsAsync(
    IReadOnlyCollection<Guid> ids,
    CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _context.ApplicationUsers
            .AsNoTracking()
            .Where(x => ids.Contains(x.UserId))
            .ToListAsync(cancellationToken);
    }

    public Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail =
            email.Trim().ToLower();

        return _context.ApplicationUsers
            .Include(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
            .Include(x => x.Discipline)
            .FirstOrDefaultAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeUserId,
        CancellationToken cancellationToken)
    {
        var normalizedEmail =
            email.Trim().ToLower();

        return _context.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Email == normalizedEmail &&
                    (!excludeUserId.HasValue ||
                     x.UserId != excludeUserId.Value),
                cancellationToken);
    }

    public Task<bool> ExistsByEmployeeNoAsync(
        string employeeNo,
        Guid? excludeUserId,
        CancellationToken cancellationToken)
    {
        var normalizedEmployeeNo =
            employeeNo.Trim();

        return _context.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.EmployeeNo == normalizedEmployeeNo &&
                    (!excludeUserId.HasValue ||
                     x.UserId != excludeUserId.Value),
                cancellationToken);
    }

    public Task<List<ApplicationUser>> GetAllAsync(
        CancellationToken cancellationToken) =>
        _context.ApplicationUsers
            .Include(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
            .Include(x => x.Discipline)
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

    public Task AddAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        _context.ApplicationUsers
            .AddAsync(
                user,
                cancellationToken)
            .AsTask();

    public void Update(
        ApplicationUser user) =>
        _context.ApplicationUsers.Update(user);

    public void Remove(
        ApplicationUser user) =>
        _context.ApplicationUsers.Remove(user);
}