using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Users.DTOs;

namespace CCAP.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _users;

    public GetUserByIdQueryHandler(
        IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserDto?> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user =
            await _users.GetByIdAsync(
                request.UserId,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

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
            user.Role?.RoleName ?? string.Empty,
            user.Discipline?.Name ?? string.Empty,
            null);
    }
}