using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteCare;

public sealed record CompleteCareCommand(
    Guid PatientId,
    Guid FinalizedByUserId,
    string FinalStatus) : IRequest;
