using MediatR;
using CCAP.Application.Abstractions.Persistence;
namespace CCAP.Application.Features.Users.Commands.DeactivateUser;
public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    public DeactivateUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }
    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        user.Deactivate();
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
