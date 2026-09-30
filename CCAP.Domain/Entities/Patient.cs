using CCAP.Domain.Enums;

namespace CCAP.Domain.Entities;

public sealed class Patient
{
    private Patient()
    {
    }

    public Guid PatientId { get; private set; }

    public string MRN { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string MiddleName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public DateOnly? DateOfBirth { get; private set; }

    public string? Gender { get; private set; }

    public string? PrimaryDiagnosis { get; private set; }

    public string? SecondaryDiagnosis { get; private set; }

    public string? Address { get; private set; }

    public string? City { get; private set; }

    public string? State { get; private set; }

    public string? ZipCode { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? AlternatePhone { get; private set; }

    public string? EmergencyContactName { get; private set; }

    public string? EmergencyContactRelationship { get; private set; }

    public string? EmergencyContactPhone { get; private set; }

    public string? PrimaryInsurance { get; private set; }

    public string? InsuranceMemberId { get; private set; }

    public DateOnly? AuthorizationDate { get; private set; }

    // Workflow tracker fields reflected from the Excel workflow.
    public DateOnly? PreAuthDueDate { get; private set; }
    public int? NumberOfVisits { get; private set; }
    public string? CaseMixType { get; private set; }
    public string? DmeMedSupplyNotes { get; private set; }
    public string? SocFeedbackFromPatient { get; private set; }
    public DateOnly? TifDate { get; private set; }
    public DateOnly? RocDate { get; private set; }
    public DateOnly? RecertDate { get; private set; }
    public bool? PcpPtNotified { get; private set; }
    public DateOnly? DischargeDate { get; private set; }
    public string? DischargeFeedback { get; private set; }
    public string? TransferDestination { get; private set; }
    public DateOnly? TransferDate { get; private set; }
    public string? TransferReason { get; private set; }

    public int? ApprovedVisits { get; private set; }

    public bool AuthorizationRequired { get; private set; }

    public DateTime? InsuranceVerifiedAt { get; private set; }

    public Guid? InsuranceVerifiedByUserId { get; private set; }

    public string? ReferringPhysician { get; private set; }

    public string? PhysicianPhone { get; private set; }

    public string? ReferralNotes { get; private set; }

    public PatientStatus Status { get; private set; }

    public Guid? CoordinatorId { get; private set; }

    public Guid? ClinicianId { get; private set; }

    public DateOnly? SocDate { get; private set; }

    public DateTime? CareCompletedAt { get; private set; }

    public Guid? FinalizedByUserId { get; private set; }

    public string? FinalStatus { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public Guid? ArchivedByUserId { get; private set; }

    public ApplicationUser? Coordinator { get; private set; }

    public ApplicationUser? Clinician { get; private set; }

    public ICollection<Referral> Referrals { get; private set; }
        = new List<Referral>();

    public ICollection<CallNote> CallNotes { get; private set; }
        = new List<CallNote>();

    public ICollection<Assessment> Assessments { get; private set; }
        = new List<Assessment>();

    public ICollection<ComplianceRecord> ComplianceRecords { get; private set; }
        = new List<ComplianceRecord>();

    public ICollection<PatientTask> Tasks { get; private set; }
        = new List<PatientTask>();

    public ICollection<Activity> Activities { get; private set; }
        = new List<Activity>();

    public ICollection<Visit> Visits { get; private set; }
        = new List<Visit>();

    public Patient(
        string mrn,
        string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(mrn))
            throw new ArgumentException(
                "MRN is required.",
                nameof(mrn));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException(
                "First name is required.",
                nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException(
                "Last name is required.",
                nameof(lastName));

        PatientId = Guid.NewGuid();

        MRN = mrn.Trim();

        FirstName = firstName.Trim();

        LastName = lastName.Trim();

        Status = PatientStatus.Active;
    }

    public void ApplyReferralIntake(
        string? middleName,
        DateOnly? dateOfBirth,
        string? gender,
        string? primaryDiagnosis,
        string? secondaryDiagnosis,
        string? address,
        string? city,
        string? state,
        string? zipCode,
        string? phoneNumber,
        string? alternatePhone,
        string? emergencyContactName,
        string? emergencyContactRelationship,
        string? emergencyContactPhone,
        string? primaryInsurance,
        string? insuranceMemberId,
        DateOnly? authorizationDate,
        int? approvedVisits,
        bool authorizationRequired,
        string? referringPhysician,
        string? physicianPhone,
        string? referralNotes,
        Guid? coordinatorId,
        Guid? clinicianId,
        DateOnly? socDate)
    {
        MiddleName = middleName?.Trim() ?? string.Empty;

        DateOfBirth = dateOfBirth;

        Gender = Normalize(gender);

        PrimaryDiagnosis = Normalize(primaryDiagnosis);

        SecondaryDiagnosis = Normalize(secondaryDiagnosis);

        Address = Normalize(address);

        City = Normalize(city);

        State = Normalize(state);

        ZipCode = Normalize(zipCode);

        PhoneNumber = Normalize(phoneNumber);

        AlternatePhone = Normalize(alternatePhone);

        EmergencyContactName =
            Normalize(emergencyContactName);

        EmergencyContactRelationship =
            Normalize(emergencyContactRelationship);

        EmergencyContactPhone =
            Normalize(emergencyContactPhone);

        PrimaryInsurance =
            Normalize(primaryInsurance);

        InsuranceMemberId =
            Normalize(insuranceMemberId);

        AuthorizationDate =
            authorizationDate;

        ApprovedVisits =
            approvedVisits;

        AuthorizationRequired =
            authorizationRequired;

        ReferringPhysician =
            Normalize(referringPhysician);

        PhysicianPhone =
            Normalize(physicianPhone);

        ReferralNotes =
            Normalize(referralNotes);

        CoordinatorId = coordinatorId;

        ClinicianId = clinicianId;

        SocDate = socDate;
    }

    public void UpdateBasicInformation(
        string mrn,
        string firstName,
        string? middleName,
        string lastName,
        DateOnly? socDate)
    {
        if (string.IsNullOrWhiteSpace(mrn))
            throw new ArgumentException("MRN is required.", nameof(mrn));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        MRN = mrn.Trim();
        FirstName = firstName.Trim();
        MiddleName = middleName?.Trim() ?? string.Empty;
        LastName = lastName.Trim();
        SocDate = socDate;
    }

    public void UpdateInsurance(
    string? primaryInsurance,
    string? insuranceMemberId,
    DateOnly? authorizationDate,
    int? approvedVisits,
    bool authorizationRequired)
    {
        PrimaryInsurance =
            string.IsNullOrWhiteSpace(primaryInsurance)
                ? null
                : primaryInsurance.Trim();

        InsuranceMemberId =
            string.IsNullOrWhiteSpace(insuranceMemberId)
                ? null
                : insuranceMemberId.Trim();

        AuthorizationDate =
            authorizationDate;

        ApprovedVisits =
            approvedVisits;

        AuthorizationRequired =
            authorizationRequired;
    }

    public void VerifyInsurance(Guid verifiedByUserId)
    {
        if (string.IsNullOrWhiteSpace(PrimaryInsurance))
        {
            throw new InvalidOperationException(
                "Primary insurance is required before insurance can be verified.");
        }

        InsuranceVerifiedAt =
            DateTime.UtcNow;

        InsuranceVerifiedByUserId =
            verifiedByUserId;
    }

    public void RequireInsuranceReverification()
    {
        InsuranceVerifiedAt = null;

        InsuranceVerifiedByUserId = null;
    }

    public void UpdateContact(
        string? address,
        string? phoneNumber)
    {
        Address = address;

        PhoneNumber = phoneNumber;
    }

    public void SetCoordinator(Guid? userId)
        => CoordinatorId = userId;

    public void SetClinician(Guid? userId)
        => ClinicianId = userId;

    public void SetSocDate(DateOnly? date)
        => SocDate = date;

    public void UpdateWorkflowDetails(
        DateOnly? preAuthDueDate,
        int? numberOfVisits,
        string? caseMixType,
        string? dmeMedSupplyNotes,
        string? socFeedbackFromPatient,
        DateOnly? tifDate,
        DateOnly? rocDate,
        DateOnly? recertDate,
        bool? pcpPtNotified,
        DateOnly? dischargeDate,
        string? dischargeFeedback,
        string? transferDestination,
        DateOnly? transferDate,
        string? transferReason)
    {
        if (numberOfVisits.HasValue && numberOfVisits.Value < 0)
            throw new ArgumentException("Number of visits cannot be negative.", nameof(numberOfVisits));

        PreAuthDueDate = preAuthDueDate;
        NumberOfVisits = numberOfVisits;
        CaseMixType = Normalize(caseMixType);
        DmeMedSupplyNotes = Normalize(dmeMedSupplyNotes);
        SocFeedbackFromPatient = Normalize(socFeedbackFromPatient);
        TifDate = tifDate;
        RocDate = rocDate;
        RecertDate = recertDate;
        PcpPtNotified = pcpPtNotified;
        DischargeDate = dischargeDate;
        DischargeFeedback = Normalize(dischargeFeedback);
        TransferDestination = Normalize(transferDestination);
        TransferDate = transferDate;
        TransferReason = Normalize(transferReason);
    }

    public void CompleteCare(
        string finalStatus,
        Guid finalizedByUserId)
    {
        if (Status != PatientStatus.Active)
            throw new InvalidOperationException(
                "Only an active patient can complete care.");

        if (string.IsNullOrWhiteSpace(finalStatus))
            throw new ArgumentException(
                "Final status is required.",
                nameof(finalStatus));

        FinalStatus = finalStatus.Trim();

        FinalizedByUserId = finalizedByUserId;

        CareCompletedAt = DateTime.UtcNow;

        // A permanent transfer is a terminal episode outcome, but it is not
        // a discharge. Keep DischargeDate empty for transferred episodes.
        if (!string.Equals(FinalStatus, "Transferred", StringComparison.OrdinalIgnoreCase))
            DischargeDate ??= DateOnly.FromDateTime(DateTime.UtcNow);

        Status = PatientStatus.Completed;
    }

    public void Archive(Guid archivedByUserId)
    {
        if (Status != PatientStatus.Completed)
            throw new InvalidOperationException(
                "Only a patient with completed care can be archived.");

        ArchivedByUserId = archivedByUserId;

        ArchivedAt = DateTime.UtcNow;

        Status = PatientStatus.Archived;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}