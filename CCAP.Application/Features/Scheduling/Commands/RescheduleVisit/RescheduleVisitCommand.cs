using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.RescheduleVisit;

public sealed record RescheduleVisitCommand(
    Guid UserId,
    Guid VisitId,
    DateTime TargetDate,
    bool CanManageAllSchedules,
    bool IsCareCoordinator,
    bool IsAdministrator = false) : IRequest;

public sealed class RescheduleVisitCommandHandler : IRequestHandler<RescheduleVisitCommand>
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleVisitCommandHandler(IVisitRepository visits, IUnitOfWork unitOfWork)
    {
        _visits = visits;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RescheduleVisitCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || request.VisitId == Guid.Empty)
            throw new ArgumentException("A valid user and visit are required.");
        if (request.TargetDate == default)
            throw new ArgumentException("A valid target date is required.", nameof(request.TargetDate));

        var visit = await _visits.GetByIdForUpdateAsync(request.VisitId, cancellationToken);
        var isAssignedToCoordinator = visit is not null &&
            (visit.CareCoordinatorId == request.UserId ||
             (!visit.CareCoordinatorId.HasValue && visit.AssignedUserId == request.UserId));
        var canAccessVisit = visit is not null &&
            (request.IsAdministrator
                ? isAssignedToCoordinator
                : request.CanManageAllSchedules ||
                  (request.IsCareCoordinator && isAssignedToCoordinator) ||
                  (!request.IsCareCoordinator && visit.AssignedUserId == request.UserId));

        if (!canAccessVisit || visit is null)
            throw new KeyNotFoundException("Visit not found or not accessible.");

        // Move the visit to the selected day while retaining its start time.
        visit.Reschedule(request.TargetDate.Date.Add(visit.ScheduledDate.TimeOfDay));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
