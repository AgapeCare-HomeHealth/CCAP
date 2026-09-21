using MediatR;

namespace CCAP.Application.Features.Scheduling.Queries.GetCalendarVisits;

public sealed record GetCalendarVisitsQuery(
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate) : IRequest<IReadOnlyList<CalendarVisitDto>>;

public sealed class CalendarVisitDto
{
    public Guid VisitId { get; set; }
    public Guid? PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Clinician { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
