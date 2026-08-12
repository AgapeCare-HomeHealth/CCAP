using MediatR;
using CCAP.Application.Features.Users.DTOs;

namespace CCAP.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string EmployeeNo,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? MobileNo,
    Guid RoleId,
    Guid? DisciplineId) : IRequest<UserDto>;
