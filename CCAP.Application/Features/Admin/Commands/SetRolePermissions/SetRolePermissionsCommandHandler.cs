using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Admin.Commands.SetRolePermissions;

public sealed class SetRolePermissionsCommandHandler : IRequestHandler<SetRolePermissionsCommand>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public SetRolePermissionsCommandHandler(
        IRoleRepository roles,
        IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        SetRolePermissionsCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        if (!role.IsActive)
            throw new InvalidOperationException("Cannot modify permissions for an inactive role.");

        await _roles.ReplacePermissionsAsync(
            request.RoleId,
            request.PermissionIds,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
