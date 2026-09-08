using MediatR;

namespace CCAP.Application.Features.Patients.Commands.ScheduleSoc;

public sealed record ScheduleSocCommand(
    Guid PatientId,
    DateOnly SocDate,
    Guid ScheduledByUserId) : IRequest;