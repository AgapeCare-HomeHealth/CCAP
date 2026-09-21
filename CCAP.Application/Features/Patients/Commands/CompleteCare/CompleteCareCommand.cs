using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteCare;

public sealed record CompleteCareCommand(
    Guid PatientId,
    string FinalStatus,
    Guid FinalizedByUserId,
    DateOnly? OutcomeDate = null,
    string? TransferDestination = null,
    string? TransferReason = null,
    string? DischargeFeedback = null) : IRequest;
