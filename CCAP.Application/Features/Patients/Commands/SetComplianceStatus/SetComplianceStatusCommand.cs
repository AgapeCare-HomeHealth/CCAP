using MediatR;
namespace CCAP.Application.Features.Patients.Commands.SetComplianceStatus;
public sealed record SetComplianceStatusCommand(Guid PatientId, string RequirementCode, string Status, Guid UserId) : IRequest;
