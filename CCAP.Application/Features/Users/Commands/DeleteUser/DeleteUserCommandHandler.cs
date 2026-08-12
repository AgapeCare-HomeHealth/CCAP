using MediatR;
using CCAP.Application.Abstractions.Persistence;
namespace CCAP.Application.Features.Users.Commands.DeleteUser;
public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        _users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
