using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    public Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.ApplicationUsers
            .Include(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
            .Include(x => x.Discipline)
            .FirstOrDefaultAsync(x => x.UserId == id, cancellationToken);

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        _context.ApplicationUsers
            .Include(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
            .Include(x => x.Discipline)
            .FirstOrDefaultAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
        _context.ApplicationUsers.AnyAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);

    public Task<bool> ExistsByEmployeeNoAsync(string employeeNo, CancellationToken cancellationToken) =>
        _context.ApplicationUsers.AnyAsync(x => x.EmployeeNo == employeeNo, cancellationToken);

    public Task<List<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken) =>
        _context.ApplicationUsers
            .Include(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
            .Include(x => x.Discipline)
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

    public Task AddAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        _context.ApplicationUsers.AddAsync(user, cancellationToken).AsTask();

    public void Update(ApplicationUser user) => _context.ApplicationUsers.Update(user);
    public void Remove(ApplicationUser user) => _context.ApplicationUsers.Remove(user);
}
