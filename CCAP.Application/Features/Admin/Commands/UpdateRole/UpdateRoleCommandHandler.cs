using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Admin.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
            throw new ArgumentException("Role name is required.");

        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        var all = await _roles.GetAllAsync(cancellationToken);
        if (all.Any(x => x.RoleId != request.RoleId && string.Equals(x.RoleName, request.RoleName.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A role with this name already exists.");

        role.Update(request.RoleName, request.Description);
        if (request.IsActive) role.Activate(); else role.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
