using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Users.DTOs;

namespace CCAP.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _users;
    public GetUsersQueryHandler(IUserRepository users) => _users = users;

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.GetAllAsync(cancellationToken);

        return users.Select(x => new UserDto(
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
            null)).ToList();
    }
}
