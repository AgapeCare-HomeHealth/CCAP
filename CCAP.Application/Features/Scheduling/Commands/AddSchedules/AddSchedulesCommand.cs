using MediatR;

namespace CCAP.Application.Features.Scheduling.Commands.AddSchedules;

public sealed record AddSchedulesCommand(
    Guid ScheduledByUserId,
    IReadOnlyList<AddScheduleItem> Schedules) : IRequest<AddSchedulesResult>;

public sealed record AddScheduleItem(
    DateTime? Date,
    string TimeBlock,
    string PatientName,
    string ConfirmationStatus,
    string? CallNotes,
    Guid? AssignedClinicianId,
    string VisitType,
    string? VisitStatus,
    string? NotesFlag,
    Guid? CareCoordinatorId = null,
    Guid? RecurrenceGroupId = null,
    int VisitsEachDay = 1,
    string? Skills = null,
    string? TreatmentAuthorizationCode = null,
    bool FlexibleVisit = false,
    bool OutsourcedVisit = false,
    string? AttachmentsNeeded = null,
    string? BillingRateCode = null,
    string? BillingRevenueCode = null,
    decimal? BillingAmount = null,
    decimal? BillingOvertimeAmount = null);
