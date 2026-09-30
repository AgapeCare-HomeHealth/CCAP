using MediatR;

namespace CCAP.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string EmployeeNo,
    string FirstName,
    string LastName,
    string Email,
    string? MobileNo,
    Guid RoleId,
    Guid? DisciplineId) : IRequest;
