using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Admin.Commands.DeleteRole;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        if (role.Users.Any())
            throw new InvalidOperationException("Cannot delete a role that still has assigned users.");

        _roles.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
