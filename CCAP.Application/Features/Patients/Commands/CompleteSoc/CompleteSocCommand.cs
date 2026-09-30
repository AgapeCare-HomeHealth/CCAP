using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteSoc;

public sealed record CompleteSocCommand(
    Guid PatientId,
    Guid CompletedByUserId,
    string? Notes) : IRequest;