using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        if (!role.IsActive)
            throw new InvalidOperationException("Role is inactive.");

        user.Update(
            request.EmployeeNo,
            request.FirstName,
            request.LastName,
            request.Email,
            request.MobileNo,
            request.RoleId,
            request.DisciplineId);

        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
