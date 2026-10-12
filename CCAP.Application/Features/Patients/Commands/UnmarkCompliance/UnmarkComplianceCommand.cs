using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UnmarkCompliance;
public sealed record UnmarkComplianceCommand(Guid PatientId, string RequirementCode, Guid UserId) : IRequest;
