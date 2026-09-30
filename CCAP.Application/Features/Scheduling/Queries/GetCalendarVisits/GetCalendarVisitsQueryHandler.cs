using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Scheduling.Queries.GetCalendarVisits;

public sealed class GetCalendarVisitsQueryHandler
    : IRequestHandler<GetCalendarVisitsQuery, IReadOnlyList<CalendarVisitDto>>
{
    private readonly IVisitRepository _visits;

    public GetCalendarVisitsQueryHandler(IVisitRepository visits)
    {
        _visits = visits;
    }

    public async Task<IReadOnlyList<CalendarVisitDto>> Handle(
        GetCalendarVisitsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("End date must be after start date.");

        var visits = await _visits.GetCalendarVisitsAsync(
            request.UserId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        return visits.Select(x => new CalendarVisitDto
        {
            VisitId = x.VisitId,
            PatientId = x.PatientId,
            PatientName = x.Patient is null
                ? x.PatientName
                : $"{x.Patient.FirstName} {x.Patient.LastName}".Trim(),
            ScheduledDate = x.ScheduledDate,
            CompletedDate = x.CompletedDate,
            Status = x.Status,
            TimeBlock = x.TimeBlock,
            ConfirmationStatus = x.ConfirmationStatus,
            CallNotes = x.CallNotes,
            Clinician = x.Clinician is null
                ? (x.ClinicianName ?? "Unassigned")
                : $"{x.Clinician.FirstName} {x.Clinician.LastName}".Trim(),
            NotesFlag = x.NotesFlag,
            Notes = x.Notes
        }).ToList();
    }
}
