namespace CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;

public sealed record CreateReferralIntakeResult(
    Guid PatientId,
    Guid ReferralId,
    string ReferralNumber,
    // PDF is temporarily optional
    Guid? ReferralDocumentId,
    string? StorageKey);