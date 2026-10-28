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
            request.CanViewAllSchedules,
            request.IsCareCoordinator,
            cancellationToken);

        return visits.Select(x => new CalendarVisitDto
        {
            VisitId = x.VisitId,
            AssignedUserId = x.AssignedUserId,
            PatientId = x.PatientId,
            CareCoordinatorId = x.CareCoordinatorId,
            CareCoordinatorName = x.CareCoordinator is null ? null : (x.CareCoordinator.FirstName + " " + x.CareCoordinator.LastName).Trim(),
            ClinicianId = x.ClinicianId,
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
            VisitType = x.VisitType,
            NotesFlag = x.NotesFlag,
            Notes = x.Notes,
            RecurrenceGroupId = x.RecurrenceGroupId,
            VisitsEachDay = x.VisitsEachDay,
            Skills = x.Skills,
            TreatmentAuthorizationCode = x.TreatmentAuthorizationCode,
            FlexibleVisit = x.FlexibleVisit,
            OutsourcedVisit = x.OutsourcedVisit,
            AttachmentsNeeded = x.AttachmentsNeeded,
            BillingRateCode = x.BillingRateCode,
            BillingRevenueCode = x.BillingRevenueCode,
            BillingAmount = x.BillingAmount,
            BillingOvertimeAmount = x.BillingOvertimeAmount
        }).ToList();
    }
}
