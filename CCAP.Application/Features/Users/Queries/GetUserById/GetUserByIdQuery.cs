using MediatR;
using CCAP.Application.Features.Users.DTOs;

namespace CCAP.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
