namespace CCAP.Application.Features.Referrals.DTOs;

public sealed class ReferralCoordinatorFilterOptionDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
