using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteSocCompliance;

public sealed record CompleteSocComplianceCommand(
    Guid PatientId,
    Guid VerifiedByUserId) : IRequest;