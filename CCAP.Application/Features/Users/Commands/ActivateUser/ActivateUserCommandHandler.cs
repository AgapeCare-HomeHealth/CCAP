using MediatR;
using CCAP.Application.Abstractions.Persistence;
namespace CCAP.Application.Features.Users.Commands.ActivateUser;
public sealed class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    public ActivateUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }
    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        user.Activate();
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
