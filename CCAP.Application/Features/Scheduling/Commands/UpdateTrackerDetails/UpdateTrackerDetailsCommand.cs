using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.UpdateTrackerDetails;

public sealed record UpdateTrackerDetailsCommand(Guid UserId, Guid VisitId, string ConfirmationStatus, string? CallNotes, Guid? ClinicianId) : IRequest;

public sealed class UpdateTrackerDetailsCommandHandler : IRequestHandler<UpdateTrackerDetailsCommand>
{
    private readonly IVisitRepository _visits;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILookupOptionRepository _lookups;

    public UpdateTrackerDetailsCommandHandler(IVisitRepository visits, IUserRepository users, IUnitOfWork unitOfWork, ILookupOptionRepository lookups)
    { _visits = visits; _users = users; _unitOfWork = unitOfWork; _lookups = lookups; }

    public async Task Handle(UpdateTrackerDetailsCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visits.GetByIdForUpdateAsync(request.VisitId, cancellationToken);
        if (visit is null || visit.AssignedUserId != request.UserId)
            throw new KeyNotFoundException("Visit not found.");
        var allowed = await _lookups.GetAsync("ConfirmationStatus", true, cancellationToken);
        if (!allowed.Any(x => string.Equals(x.DisplayName, request.ConfirmationStatus, StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(x.Code, request.ConfirmationStatus, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Select a valid confirmation status.");
        string? clinicianName = null;
        if (request.ClinicianId.HasValue)
        {
            var users = await _users.GetAllAsync(cancellationToken);
            var clinician = users.FirstOrDefault(x => x.UserId == request.ClinicianId && x.IsActive && x.Role is not null &&
                string.Equals(x.Role.RoleName, "Clinician", StringComparison.OrdinalIgnoreCase));
            if (clinician is null) throw new ArgumentException("Select an active clinician.");
            clinicianName = $"{clinician.FirstName} {clinician.LastName}".Trim();
        }
        visit.UpdateTrackerDetails(request.ConfirmationStatus, request.CallNotes, request.ClinicianId, clinicianName);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
