namespace CCAP.Domain.Entities;

public sealed class ReferralTrackerEntry
{
    public Guid ReferralTrackerEntryId { get; private set; } = Guid.NewGuid();
    public string ReferralNumber { get; private set; } = string.Empty;
    public DateTime ReferralDate { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly? DateOfBirth { get; private set; }
    public string Mrn { get; private set; } = string.Empty;
    public string? TypeOfReferral { get; private set; }
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
        string lastName, string typeOfReferral, string? postedOn, DateOnly? edd,
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
        TypeOfReferral = NormalizeTypeOfReferral(typeOfReferral);
        Update(postedOn, edd, status, source, insurance, assignedCoordinatorId,
            assignedCoordinatorName, firstMarketingCall, secondCall, thirdCall, resolution, typeOfReferral);
    }

    public void Update(string? postedOn, DateOnly? edd, string? status, string? source,
        string? insurance, Guid? assignedCoordinatorId, string? assignedCoordinatorName,
        string? firstMarketingCall, string? secondCall, string? thirdCall, string? resolution,
        string? typeOfReferral = null)
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
        if (typeOfReferral is not null)
            TypeOfReferral = string.IsNullOrWhiteSpace(typeOfReferral)
                ? null
                : NormalizeTypeOfReferral(typeOfReferral);
        UpdatedAt = DateTime.UtcNow;
    }

    public static bool IsValidTypeOfReferral(string? value) =>
        value?.Trim().Equals("New Patient", StringComparison.OrdinalIgnoreCase) == true ||
        value?.Trim().Equals("Previous Patient", StringComparison.OrdinalIgnoreCase) == true;

    private static string NormalizeTypeOfReferral(string value)
    {
        var normalized = value.Trim();
        if (normalized.Equals("New Patient", StringComparison.OrdinalIgnoreCase))
            return "New Patient";
        if (normalized.Equals("Previous Patient", StringComparison.OrdinalIgnoreCase))
            return "Previous Patient";
        throw new ArgumentException("Type of referral must be New Patient or Previous Patient.", nameof(value));
    }
}
