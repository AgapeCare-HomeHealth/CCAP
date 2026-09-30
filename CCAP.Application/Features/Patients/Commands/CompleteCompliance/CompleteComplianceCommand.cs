using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteCompliance;

public sealed record CompleteComplianceCommand(
    Guid PatientId,
    string RequirementCode,
    Guid CompletedByUserId) : IRequest;