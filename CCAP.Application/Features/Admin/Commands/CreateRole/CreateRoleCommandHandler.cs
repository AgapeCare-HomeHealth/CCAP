using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.DTOs;
using CCAP.Domain.Entities;

namespace CCAP.Application.Features.Admin.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleListItemDto>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleListItemDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
            throw new ArgumentException("Role name is required.");

        var existing = await _roles.GetAllAsync(cancellationToken);
        if (existing.Any(x => string.Equals(x.RoleName, request.RoleName.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A role with this name already exists.");

        var role = new Role(request.RoleName, request.Description);
        if (!request.IsActive)
            role.Deactivate();

        await _roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleListItemDto(role.RoleId, role.RoleName, role.Description ?? string.Empty, 0, 0, role.IsActive);
    }
}
