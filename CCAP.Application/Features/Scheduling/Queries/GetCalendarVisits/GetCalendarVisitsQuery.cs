using MediatR;

namespace CCAP.Application.Features.Scheduling.Queries.GetCalendarVisits;

public sealed record GetCalendarVisitsQuery(
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate,
    bool CanViewAllSchedules = false,
    bool IsCareCoordinator = false) : IRequest<IReadOnlyList<CalendarVisitDto>>;

public sealed class CalendarVisitDto
{
    public Guid VisitId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? CareCoordinatorId { get; set; }
    public string? CareCoordinatorName { get; set; }
    public Guid? ClinicianId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TimeBlock { get; set; }
    public string? ConfirmationStatus { get; set; }
    public string? CallNotes { get; set; }
    public string Clinician { get; set; } = string.Empty;
    public string VisitType { get; set; } = "Visit";
    public string? NotesFlag { get; set; }
    public string? Notes { get; set; }
    public Guid? RecurrenceGroupId { get; set; }
    public int VisitsEachDay { get; set; } = 1;
    public string? Skills { get; set; }
    public string? TreatmentAuthorizationCode { get; set; }
    public bool FlexibleVisit { get; set; }
    public bool OutsourcedVisit { get; set; }
    public string? AttachmentsNeeded { get; set; }
    public string? BillingRateCode { get; set; }
    public string? BillingRevenueCode { get; set; }
    public decimal? BillingAmount { get; set; }
    public decimal? BillingOvertimeAmount { get; set; }
}
