using MediatR;
namespace CCAP.Application.Features.Users.Commands.ActivateUser;
public sealed record ActivateUserCommand(Guid UserId) : IRequest;
