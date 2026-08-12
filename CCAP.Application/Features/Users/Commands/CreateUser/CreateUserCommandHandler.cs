using MediatR;
using CCAP.Application.Abstractions.Identity;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Users.DTOs;
using CCAP.Domain.Entities;

namespace CCAP.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email already exists.");

        if (await _users.ExistsByEmployeeNoAsync(request.EmployeeNo, cancellationToken))
            throw new InvalidOperationException("Employee number already exists.");

        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null || !role.IsActive)
            throw new InvalidOperationException("Invalid or inactive role.");

        var user = new ApplicationUser(
            request.EmployeeNo,
            request.FirstName,
            request.LastName,
            request.Email,
            string.Empty,
            request.RoleId,
            request.DisciplineId,
            request.MobileNo);

        user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.UserId,
            user.EmployeeNo,
            user.FirstName,
            user.LastName,
            user.Email,
            user.MobileNo,
            user.IsActive,
            user.RoleId,
            user.DisciplineId,
            role.RoleName,
            user.Discipline?.Name ?? string.Empty,
            null);
    }
}
