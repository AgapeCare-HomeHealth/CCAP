using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.AssignCareCoordinators;

public sealed record AssignCareCoordinatorsCommand(Guid UserId, Guid CareCoordinatorId, IReadOnlyList<Guid> VisitIds) : IRequest<int>;

public sealed class AssignCareCoordinatorsCommandHandler : IRequestHandler<AssignCareCoordinatorsCommand, int>
{
    private readonly IVisitRepository _visits;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    public AssignCareCoordinatorsCommandHandler(IVisitRepository visits, IUserRepository users, IUnitOfWork unitOfWork)
    { _visits = visits; _users = users; _unitOfWork = unitOfWork; }

    public async Task<int> Handle(AssignCareCoordinatorsCommand request, CancellationToken cancellationToken)
    {
        var ids = request.VisitIds?.Distinct().ToArray() ?? [];
        if (ids.Length == 0 || ids.Length > 500 || ids.Any(x => x == Guid.Empty))
            throw new ArgumentException("Select between 1 and 500 visits.");
        var users = await _users.GetAllAsync(cancellationToken);
        var cc = users.FirstOrDefault(x => x.UserId == request.CareCoordinatorId && x.IsActive && x.Role is not null &&
            string.Equals(x.Role.RoleName, "Care Coordinator", StringComparison.OrdinalIgnoreCase));
        if (cc is null) throw new ArgumentException("Select an active Care Coordinator.");
        var visits = await _visits.GetForAssignmentAsync(request.UserId, ids, cancellationToken);
        if (visits.Count != ids.Length) throw new KeyNotFoundException("One or more selected visits were not found.");
        // Never overwrite existing CC assignments in this temporary workflow.
        var assignable = visits.Where(v => !v.CareCoordinatorId.HasValue).ToList();
        foreach (var visit in assignable) visit.AssignCareCoordinator(cc.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return assignable.Count;
    }
}
