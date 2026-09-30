using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteInsuranceVerification;

public sealed record CompleteInsuranceVerificationCommand(
    Guid PatientId,
    Guid VerifiedByUserId) : IRequest;