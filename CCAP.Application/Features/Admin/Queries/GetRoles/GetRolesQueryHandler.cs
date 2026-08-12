using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetRoles;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleListItemDto>>
{
    private readonly IAdminLookupRepository _repository;
    public GetRolesQueryHandler(IAdminLookupRepository repository) => _repository = repository;

    public async Task<List<RoleListItemDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _repository.GetRolesAsync(cancellationToken);
        return roles.Select(r => new RoleListItemDto(
            r.RoleId,
            r.RoleName,
            r.Description ?? string.Empty,
            r.Users.Count,
            r.RolePermissions.Count,
            r.IsActive)).ToList();
    }
}
