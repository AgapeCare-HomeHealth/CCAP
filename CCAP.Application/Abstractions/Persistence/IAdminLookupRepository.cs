using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IAdminLookupRepository
{
    Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken);
    Task<List<Permission>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<List<Discipline>> GetDisciplinesAsync(CancellationToken cancellationToken);
}
