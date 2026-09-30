using MediatR;
namespace CCAP.Application.Features.Users.Commands.DeleteUser;
public sealed record DeleteUserCommand(Guid UserId) : IRequest;
