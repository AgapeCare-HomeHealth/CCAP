using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.DeleteScheduleVisits;

public sealed record DeleteScheduleVisitsCommand(Guid UserId, IReadOnlyList<Guid> VisitIds, bool IsAdministrator = false) : IRequest<int>;

public sealed class DeleteScheduleVisitsCommandHandler : IRequestHandler<DeleteScheduleVisitsCommand, int>
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteScheduleVisitsCommandHandler(IVisitRepository visits, IUnitOfWork unitOfWork)
    {
        _visits = visits;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(DeleteScheduleVisitsCommand request, CancellationToken cancellationToken)
    {
        var ids = request.VisitIds?.Distinct().ToArray() ?? [];
        if (ids.Length == 0 || ids.Length > 500 || ids.Any(x => x == Guid.Empty))
            throw new ArgumentException("Select between 1 and 500 visits to delete.");

        // Ownership is enforced by the repository query; refuse partial deletions.
        var visits = await _visits.GetForAssignmentAsync(request.UserId, ids, cancellationToken);
        if (visits.Count != ids.Length)
            throw new KeyNotFoundException("One or more selected visits were not found or are not accessible.");

        if (request.IsAdministrator && visits.Any(visit =>
                visit.CareCoordinatorId.HasValue && visit.CareCoordinatorId != request.UserId))
            throw new KeyNotFoundException("You cannot delete a visit assigned to another Care Coordinator.");

        _visits.RemoveRange(visits);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return visits.Count;
    }
}
