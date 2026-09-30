using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeUserId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmployeeNoAsync(
        string employeeNo,
        Guid? excludeUserId,
        CancellationToken cancellationToken);

    Task<List<ApplicationUser>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<List<ApplicationUser>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task AddAsync(
        ApplicationUser user,
        CancellationToken cancellationToken);

    void Update(ApplicationUser user);

    void Remove(ApplicationUser user);
}