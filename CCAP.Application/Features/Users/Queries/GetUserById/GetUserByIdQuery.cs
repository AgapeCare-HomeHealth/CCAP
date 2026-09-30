using CCAP.Application.Features.Users.DTOs;
using MediatR;

namespace CCAP.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(
    Guid UserId)
    : IRequest<UserDto?>;