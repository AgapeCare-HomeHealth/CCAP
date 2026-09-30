using MediatR;
using CCAP.Application.Features.Users.DTOs;

namespace CCAP.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<List<UserDto>>;
