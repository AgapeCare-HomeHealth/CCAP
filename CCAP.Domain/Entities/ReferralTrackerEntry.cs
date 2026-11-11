namespace CCAP.Domain.Entities;

public sealed class ReferralTrackerEntry
{
    public Guid ReferralTrackerEntryId { get; private set; } = Guid.NewGuid();
    public string ReferralNumber { get; private set; } = string.Empty;
    public DateTime ReferralDate { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string Mrn { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string Insurance { get; private set; } = string.Empty;
    public Guid? AssignedCoordinatorId { get; private set; }
    public string? AssignedCoordinatorName { get; private set; }
    public string PostedOn { get; private set; } = "Referral/Intake";
    public DateOnly? EstimatedDischargeDate { get; private set; }
    public string FirstMarketingCall { get; private set; } = string.Empty;
    public string SecondCoordinatorCall { get; private set; } = string.Empty;
    public string ThirdCoordinatorCall { get; private set; } = string.Empty;
    public string Resolution { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private ReferralTrackerEntry() { }

    public ReferralTrackerEntry(string referralNumber, DateTime referralDate, string firstName,
        string lastName, DateOnly dateOfBirth, string? mrn, string? postedOn, DateOnly? edd,
        string? status, string? source, string? insurance, Guid? assignedCoordinatorId,
        string? assignedCoordinatorName, string? firstMarketingCall, string? secondCall,
        string? thirdCall, string? resolution)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");
        ReferralNumber = string.IsNullOrWhiteSpace(referralNumber)
            ? $"TRK-{Guid.NewGuid():N}"[..20] : referralNumber.Trim();
        ReferralDate = referralDate;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
        Mrn = mrn?.Trim() ?? string.Empty;
        Update(postedOn, edd, status, source, insurance, assignedCoordinatorId,
            assignedCoordinatorName, firstMarketingCall, secondCall, thirdCall, resolution);
    }

    public void Update(string? postedOn, DateOnly? edd, string? status, string? source,
        string? insurance, Guid? assignedCoordinatorId, string? assignedCoordinatorName,
        string? firstMarketingCall, string? secondCall, string? thirdCall, string? resolution)
    {
        PostedOn = postedOn?.Trim() ?? string.Empty;
        EstimatedDischargeDate = edd;
        Status = status?.Trim() ?? string.Empty;
        Source = source?.Trim() ?? string.Empty;
        Insurance = insurance?.Trim() ?? string.Empty;
        if (assignedCoordinatorId.HasValue)
        {
            AssignedCoordinatorId = assignedCoordinatorId;
            AssignedCoordinatorName = assignedCoordinatorName?.Trim();
        }
        FirstMarketingCall = firstMarketingCall?.Trim() ?? string.Empty;
        SecondCoordinatorCall = secondCall?.Trim() ?? string.Empty;
        ThirdCoordinatorCall = thirdCall?.Trim() ?? string.Empty;
        Resolution = resolution?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
