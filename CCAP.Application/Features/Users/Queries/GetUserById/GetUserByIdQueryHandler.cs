using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Users.DTOs;

namespace CCAP.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _users;
    public GetUserByIdQueryHandler(IUserRepository users) => _users = users;

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var x = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (x is null) return null;

        return new UserDto(
            x.UserId,
            x.EmployeeNo,
            x.FirstName,
            x.LastName,
            x.Email,
            x.MobileNo,
            x.IsActive,
            x.RoleId,
            x.DisciplineId,
            x.Role?.RoleName ?? string.Empty,
            x.Discipline?.Name ?? string.Empty,
            null);
    }
}
