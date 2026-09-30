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
    string? NotesFlag);
