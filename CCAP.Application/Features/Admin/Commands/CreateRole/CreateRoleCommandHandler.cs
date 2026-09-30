using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.DTOs;
using CCAP.Domain.Entities;

namespace CCAP.Application.Features.Admin.Commands.CreateRole;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, RoleListItemDto>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
        IRoleRepository roles,
        IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleListItemDto> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // =========================================================
        // DUPLICATE ROLE NAME
        // =========================================================

        if (await _roles.ExistsByNameAsync(
                request.RoleName,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "A role with this name already exists.");
        }

        // =========================================================
        // CREATE ROLE
        // =========================================================

        var role = new Role(
            request.RoleName.Trim(),
            request.Description);

        // =========================================================
        // INITIAL STATUS
        // =========================================================

        if (!request.IsActive)
        {
            role.Deactivate();
        }

        // =========================================================
        // SAVE
        // =========================================================

        await _roles.AddAsync(
            role,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        // =========================================================
        // RESULT
        // =========================================================

        return new RoleListItemDto(
            role.RoleId,
            role.RoleName,
            role.Description ?? string.Empty,
            0,
            0,
            role.IsActive);
    }
}