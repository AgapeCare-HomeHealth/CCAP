using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Admin.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler
    : IRequestHandler<UpdateRoleCommand>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(
        IRoleRepository roles,
        IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // =========================================================
        // FIND ROLE
        // =========================================================

        var role =
            await _roles.GetByIdAsync(
                request.RoleId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Role not found.");

        // =========================================================
        // DUPLICATE ROLE NAME
        //
        // Exclude the current role so keeping its existing name
        // does not produce a duplicate error.
        // =========================================================

        if (await _roles.ExistsByNameAsync(
                request.RoleName,
                request.RoleId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "A role with this name already exists.");
        }

        // =========================================================
        // UPDATE DOMAIN ENTITY
        // =========================================================

        role.Update(
            request.RoleName.Trim(),
            request.Description);

        // =========================================================
        // ACTIVE STATUS
        // =========================================================

        if (request.IsActive)
        {
            role.Activate();
        }
        else
        {
            role.Deactivate();
        }

        // =========================================================
        // SAVE
        // =========================================================

        _roles.Update(role);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}