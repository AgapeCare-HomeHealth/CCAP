using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler
    : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
{
    private readonly IAdminLookupRepository _repository;

    public GetPermissionsQueryHandler(
        IAdminLookupRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PermissionDto>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions =
            await _repository.GetPermissionsAsync(
                cancellationToken);

        return permissions
            .Select(p => new PermissionDto(
                p.PermissionId,
                p.PermissionCode,
                p.PermissionName,
                p.Module,
                p.Description ?? string.Empty))
            .ToList();
    }
}