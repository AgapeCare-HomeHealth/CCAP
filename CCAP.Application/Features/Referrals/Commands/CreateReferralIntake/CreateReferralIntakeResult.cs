namespace CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;

public sealed record CreateReferralIntakeResult(
    Guid PatientId,
    Guid ReferralId,
    string ReferralNumber,
    Guid ReferralDocumentId,
    string StorageKey);