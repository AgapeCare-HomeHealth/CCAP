using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.UpdateCalendarVisitDetails;

public sealed record UpdateCalendarVisitDetailsCommand(
    Guid UserId,
    Guid VisitId,
    string TimeBlock,
    string ConfirmationStatus,
    string? CallNotes,
    Guid? ClinicianId,
    string? NotesFlag,
    string Status,
    bool CanViewAllSchedules,
    bool IsCareCoordinator,
    bool IsAdministrator = false) : IRequest;

public sealed class UpdateCalendarVisitDetailsCommandHandler : IRequestHandler<UpdateCalendarVisitDetailsCommand>
{
    private readonly IVisitRepository _visits;
    private readonly IUserRepository _users;
    private readonly ILookupOptionRepository _lookups;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCalendarVisitDetailsCommandHandler(
        IVisitRepository visits,
        IUserRepository users,
        ILookupOptionRepository lookups,
        IUnitOfWork unitOfWork)
    {
        _visits = visits;
        _users = users;
        _lookups = lookups;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCalendarVisitDetailsCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || request.VisitId == Guid.Empty)
            throw new ArgumentException("A valid user and visit are required.");

        var visit = await _visits.GetByIdForUpdateAsync(request.VisitId, cancellationToken);
        if (visit is null || !CanEditVisit(visit, request))
            throw new KeyNotFoundException("Visit not found or not accessible.");

        var confirmationStatuses = await _lookups.GetAsync("ConfirmationStatus", true, cancellationToken);
        var allowedConfirmationStatuses = new[] { "Confirmed", "Left VM", "Sent SMS", "No Answer", "Rescheduled", "Transferred/TIF", "Cancelled Visit", "Non-Visit Discharge", "Missed Visit", "Refused Visit", "labs" };
        if (!confirmationStatuses.Any(x =>
                string.Equals(x.DisplayName, request.ConfirmationStatus, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Code, request.ConfirmationStatus, StringComparison.OrdinalIgnoreCase)) &&
            !allowedConfirmationStatuses.Contains(request.ConfirmationStatus, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Select a valid confirmation status.");

        string? clinicianName = null;
        if (request.ClinicianId.HasValue)
        {
            var users = await _users.GetAllAsync(cancellationToken);
            var clinician = users.FirstOrDefault(x => x.UserId == request.ClinicianId.Value && x.IsActive && x.Role is not null &&
                string.Equals(x.Role.RoleName, "Clinician", StringComparison.OrdinalIgnoreCase));
            if (clinician is null)
                throw new ArgumentException("Select an active clinician.");

            clinicianName = $"{clinician.FirstName} {clinician.LastName}".Trim();
        }

        visit.UpdateCalendarDetails(
            request.TimeBlock,
            request.ConfirmationStatus,
            request.CallNotes,
            request.ClinicianId,
            clinicianName,
            request.NotesFlag,
            request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool CanEditVisit(Visit visit, UpdateCalendarVisitDetailsCommand request)
    {
        // Admins can view all schedules, but cannot edit another CC's patients.
        if (request.IsAdministrator)
        {
            return visit.CareCoordinatorId == request.UserId ||
                   (!visit.CareCoordinatorId.HasValue && visit.AssignedUserId == request.UserId);
        }

        if (request.CanViewAllSchedules)
            return true;

        if (request.IsCareCoordinator)
        {
            return visit.CareCoordinatorId == request.UserId ||
                   (!visit.CareCoordinatorId.HasValue && visit.AssignedUserId == request.UserId);
        }

        return visit.AssignedUserId == request.UserId;
    }
}
