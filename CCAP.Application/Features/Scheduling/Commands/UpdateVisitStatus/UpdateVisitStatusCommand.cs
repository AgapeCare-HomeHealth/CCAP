using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.UpdateVisitStatus;

public sealed record UpdateVisitStatusCommand(Guid UserId, Guid VisitId, string Status, bool IsAdministrator = false) : IRequest;

public sealed class UpdateVisitStatusCommandHandler : IRequestHandler<UpdateVisitStatusCommand>
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVisitStatusCommandHandler(IVisitRepository visits, IUnitOfWork unitOfWork)
    {
        _visits = visits;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateVisitStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || request.VisitId == Guid.Empty)
            throw new ArgumentException("A valid user and visit are required.");

        var visit = await _visits.GetByIdForUpdateAsync(request.VisitId, cancellationToken);
        if (visit is null || visit.AssignedUserId != request.UserId ||
            (request.IsAdministrator && visit.CareCoordinatorId.HasValue && visit.CareCoordinatorId != request.UserId))
            throw new KeyNotFoundException("Visit not found or not accessible.");

        visit.ChangeVisitStatus(request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
