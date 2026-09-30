using MediatR;
using CCAP.Application.Abstractions.Persistence;

namespace CCAP.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand>
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

    public async Task Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        // =========================================================
        // FIND USER
        // =========================================================

        var user =
            await _users.GetByIdAsync(
                request.UserId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "User not found.");

        // =========================================================
        // CHECK ROLE
        // =========================================================

        var role =
            await _roles.GetByIdAsync(
                request.RoleId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Role not found.");

        if (!role.IsActive)
        {
            throw new InvalidOperationException(
                "Role is inactive.");
        }

        // =========================================================
        // CHECK DUPLICATE EMAIL
        //
        // Exclude the current user so that keeping their
        // existing email does not trigger a duplicate error.
        // =========================================================

        if (await _users.ExistsByEmailAsync(
                request.Email,
                request.UserId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "A different user with this email already exists.");
        }

        // =========================================================
        // CHECK DUPLICATE EMPLOYEE NUMBER
        // =========================================================

        if (await _users.ExistsByEmployeeNoAsync(
                request.EmployeeNo,
                request.UserId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "A different user with this employee number already exists.");
        }

        // =========================================================
        // UPDATE DOMAIN ENTITY
        // =========================================================

        user.Update(
            request.EmployeeNo,
            request.FirstName,
            request.LastName,
            request.Email,
            request.MobileNo,
            request.RoleId,
            request.DisciplineId);

        // =========================================================
        // PERSIST
        // =========================================================

        _users.Update(user);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}