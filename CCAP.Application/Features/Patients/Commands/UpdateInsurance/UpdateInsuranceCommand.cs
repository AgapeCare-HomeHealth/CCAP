using MediatR;

namespace CCAP.Application.Features.Patients.Commands.UpdateInsurance;

public sealed record UpdateInsuranceCommand(
    Guid PatientId,
    string? PrimaryInsurance,
    string? InsuranceMemberId,
    DateOnly? AuthorizationDate,
    int? ApprovedVisits,
    bool AuthorizationRequired) : IRequest;