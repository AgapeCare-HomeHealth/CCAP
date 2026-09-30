using MediatR;
namespace CCAP.Application.Features.Users.Commands.DeactivateUser;
public sealed record DeactivateUserCommand(Guid UserId) : IRequest;
