namespace CCAP.Domain.Entities;

public sealed class Visit
{
    private Visit()
    {
    }

    public Guid VisitId { get; private set; }

    // Patient is optional for imported external schedules. The schedule can
    // exist in CCAP before the patient is created/matched.
    public Guid? PatientId { get; private set; }

    // Clinician is also optional for imported schedules because the external
    // schedule may contain only a clinician label that CCAP cannot map yet.
    public Guid? ClinicianId { get; private set; }

    // The CCAP user who owns this calendar entry. Imported schedules are
    // personalized to the user who performed the import.
    public Guid? AssignedUserId { get; private set; }

    // Temporary scheduling-level CC assignment; distinct from calendar ownership.
    public Guid? CareCoordinatorId { get; private set; }
    public ApplicationUser? CareCoordinator { get; private set; }

    public string PatientName { get; private set; } = string.Empty;
    public string? ClinicianName { get; private set; }
    public string VisitType { get; private set; } = "Visit";
    public string? TimeBlock { get; private set; }
    public string? ConfirmationStatus { get; private set; }
    public string? CallNotes { get; private set; }
    public string? NotesFlag { get; private set; }
    public string? Location { get; private set; }

    // Scheduling-form fields. One Visit row represents one selected calendar date.
    public Guid? RecurrenceGroupId { get; private set; }
    public int VisitsEachDay { get; private set; } = 1;
    public string? Skills { get; private set; }
    public string? TreatmentAuthorizationCode { get; private set; }
    public bool FlexibleVisit { get; private set; }
    public bool OutsourcedVisit { get; private set; }
    public string? AttachmentsNeeded { get; private set; }
    public string? BillingRateCode { get; private set; }
    public string? BillingRevenueCode { get; private set; }
    public decimal? BillingAmount { get; private set; }
    public decimal? BillingOvertimeAmount { get; private set; }

    public DateTime ScheduledDate { get; private set; }

    public DateTime? CompletedDate { get; private set; }

    public string Status { get; private set; } = "Scheduled";

    public string? Notes { get; private set; }

    public Patient? Patient { get; private set; }

    public ApplicationUser? Clinician { get; private set; }

    // Used by the existing CCAP workflow when a patient and clinician are
    // already known.
    public Visit(
        Guid patientId,
        Guid clinicianId,
        DateTime scheduledDate,
        Guid? assignedUserId = null)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.", nameof(patientId));

        if (clinicianId == Guid.Empty)
            throw new ArgumentException("Clinician ID is required.", nameof(clinicianId));

        VisitId = Guid.NewGuid();
        PatientId = patientId;
        ClinicianId = clinicianId;
        AssignedUserId = assignedUserId;
        ScheduledDate = scheduledDate;
        Status = "Scheduled";
    }

    // Used for schedules entered manually from the Scheduling calendar.
    // Patient identity is intentionally represented by PatientName because
    // Care Coordinators may schedule a patient who is not yet matched in CCAP.
    public static Visit CreateManualSchedule(
        string patientName,
        DateTime scheduledDate,
        string timeBlock,
        string confirmationStatus,
        string? callNotes,
        Guid clinicianId,
        string visitType,
        string? visitStatus,
        string? notesFlag,
        Guid assignedUserId,
        Guid? careCoordinatorId = null,
        Guid? recurrenceGroupId = null,
        int visitsEachDay = 1,
        string? skills = null,
        string? treatmentAuthorizationCode = null,
        bool flexibleVisit = false,
        bool outsourcedVisit = false,
        string? attachmentsNeeded = null,
        string? billingRateCode = null,
        string? billingRevenueCode = null,
        decimal? billingAmount = null,
        decimal? billingOvertimeAmount = null)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name is required.", nameof(patientName));
        if (string.IsNullOrWhiteSpace(timeBlock))
            throw new ArgumentException("Time block is required.", nameof(timeBlock));
        if (string.IsNullOrWhiteSpace(confirmationStatus))
            throw new ArgumentException("Confirmation status is required.", nameof(confirmationStatus));
        if (clinicianId == Guid.Empty)
            throw new ArgumentException("Assigned clinician is required.", nameof(clinicianId));
        if (string.IsNullOrWhiteSpace(visitType))
            throw new ArgumentException("Visit type is required.", nameof(visitType));
        if (assignedUserId == Guid.Empty)
            throw new ArgumentException("Assigned user ID is required.", nameof(assignedUserId));
        if (visitsEachDay < 1 || visitsEachDay > 24)
            throw new ArgumentOutOfRangeException(nameof(visitsEachDay), "Visits each day must be between 1 and 24.");
        if ((billingAmount.HasValue && billingAmount.Value < 0) || (billingOvertimeAmount.HasValue && billingOvertimeAmount.Value < 0))
            throw new ArgumentOutOfRangeException(nameof(billingAmount), "Billing amounts cannot be negative.");

        return new Visit
        {
            VisitId = Guid.NewGuid(),
            PatientName = patientName.Trim(),
            ClinicianId = clinicianId,
            AssignedUserId = assignedUserId,
            CareCoordinatorId = careCoordinatorId,
            RecurrenceGroupId = recurrenceGroupId,
            VisitsEachDay = visitsEachDay,
            Skills = NormalizeOptional(skills),
            TreatmentAuthorizationCode = NormalizeOptional(treatmentAuthorizationCode),
            FlexibleVisit = flexibleVisit,
            OutsourcedVisit = outsourcedVisit,
            AttachmentsNeeded = NormalizeOptional(attachmentsNeeded),
            BillingRateCode = NormalizeOptional(billingRateCode),
            BillingRevenueCode = NormalizeOptional(billingRevenueCode),
            BillingAmount = billingAmount,
            BillingOvertimeAmount = billingOvertimeAmount,
            ScheduledDate = scheduledDate.Date.Add(ParseStartTime(timeBlock)),
            TimeBlock = timeBlock.Trim(),
            ConfirmationStatus = confirmationStatus.Trim(),
            CallNotes = string.IsNullOrWhiteSpace(callNotes) ? null : callNotes.Trim(),
            VisitType = visitType.Trim(),
            Status = string.IsNullOrWhiteSpace(visitStatus) ? "Scheduled" : visitStatus.Trim(),
            NotesFlag = string.IsNullOrWhiteSpace(notesFlag) ? null : notesFlag.Trim()
        };
    }

    // Used for imported external schedules. The import uses the same Visit
    // table and the same scheduling fields as manually added schedules.
    public static Visit CreateImportedSchedule(
        string patientName,
        DateTime scheduledDate,
        string timeBlock,
        string confirmationStatus,
        string? callNotes,
        string? clinicianName,
        Guid? patientId,
        Guid? clinicianId,
        Guid assignedUserId,
        string visitType,
        string visitStatus,
        string? notesFlag)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name is required.", nameof(patientName));

        if (string.IsNullOrWhiteSpace(timeBlock))
            throw new ArgumentException("Time block is required.", nameof(timeBlock));

        if (string.IsNullOrWhiteSpace(confirmationStatus))
            throw new ArgumentException("Confirmation status is required.", nameof(confirmationStatus));

        if (assignedUserId == Guid.Empty)
            throw new ArgumentException("Assigned user ID is required.", nameof(assignedUserId));

        if (string.IsNullOrWhiteSpace(visitType))
            throw new ArgumentException("Visit type is required.", nameof(visitType));

        return new Visit
        {
            VisitId = Guid.NewGuid(),
            PatientId = patientId,
            ClinicianId = clinicianId,
            AssignedUserId = assignedUserId,
            PatientName = patientName.Trim(),
            ClinicianName = string.IsNullOrWhiteSpace(clinicianName) ? null : clinicianName.Trim(),
            VisitType = visitType.Trim(),
            TimeBlock = timeBlock.Trim(),
            ConfirmationStatus = confirmationStatus.Trim(),
            CallNotes = string.IsNullOrWhiteSpace(callNotes) ? null : callNotes.Trim(),
            NotesFlag = string.IsNullOrWhiteSpace(notesFlag) ? null : notesFlag.Trim(),
            ScheduledDate = scheduledDate.Date.Add(ParseStartTime(timeBlock)),
            Status = string.IsNullOrWhiteSpace(visitStatus) ? string.Empty : visitStatus.Trim()
        };
    }

    public void UpdateImportedSchedule(
        string patientName,
        DateTime scheduledDate,
        string timeBlock,
        string confirmationStatus,
        string? callNotes,
        string? clinicianName,
        Guid? patientId,
        Guid? clinicianId,
        Guid? assignedUserId,
        string visitType,
        string visitStatus,
        string? notesFlag)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name is required.", nameof(patientName));

        if (string.IsNullOrWhiteSpace(timeBlock))
            throw new ArgumentException("Time block is required.", nameof(timeBlock));

        if (string.IsNullOrWhiteSpace(confirmationStatus))
            throw new ArgumentException("Confirmation status is required.", nameof(confirmationStatus));

        if (string.IsNullOrWhiteSpace(visitType))
            throw new ArgumentException("Visit type is required.", nameof(visitType));

        PatientName = patientName.Trim();
        ScheduledDate = scheduledDate.Date.Add(ParseStartTime(timeBlock));
        TimeBlock = timeBlock.Trim();
        ConfirmationStatus = confirmationStatus.Trim();
        CallNotes = string.IsNullOrWhiteSpace(callNotes) ? null : callNotes.Trim();
        ClinicianName = string.IsNullOrWhiteSpace(clinicianName) ? null : clinicianName.Trim();
        PatientId = patientId;
        ClinicianId = clinicianId;
        AssignedUserId = assignedUserId;
        VisitType = visitType.Trim();
        Status = string.IsNullOrWhiteSpace(visitStatus) ? string.Empty : visitStatus.Trim();
        NotesFlag = string.IsNullOrWhiteSpace(notesFlag) ? null : notesFlag.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static TimeSpan ParseStartTime(string timeBlock)
    {
        var normalized = timeBlock.Trim().Replace('–', '-');
        var separator = normalized.IndexOf('-');
        var startText = separator < 0 ? normalized : normalized[..separator].Trim();

        if (!DateTime.TryParse(
                startText,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                out var start))
        {
            throw new ArgumentException($"Invalid time block: '{timeBlock}'.", nameof(timeBlock));
        }

        return start.TimeOfDay;
    }

    public void UpdateTrackerDetails(string confirmationStatus, string? callNotes, Guid? clinicianId, string? clinicianName)
    {
        if (string.IsNullOrWhiteSpace(confirmationStatus))
            throw new ArgumentException("Confirmation status is required.", nameof(confirmationStatus));
        if (callNotes?.Length > 4000)
            throw new ArgumentException("Call notes cannot exceed 4000 characters.", nameof(callNotes));
        ConfirmationStatus = confirmationStatus.Trim();
        CallNotes = string.IsNullOrWhiteSpace(callNotes) ? null : callNotes.Trim();
        ClinicianId = clinicianId;
        ClinicianName = string.IsNullOrWhiteSpace(clinicianName) ? null : clinicianName.Trim();
    }

    public void UpdateCalendarDetails(
        string timeBlock,
        string confirmationStatus,
        string? callNotes,
        Guid? clinicianId,
        string? clinicianName,
        string? notesFlag,
        string status)
    {
        if (string.IsNullOrWhiteSpace(timeBlock))
            throw new ArgumentException("Time block is required.", nameof(timeBlock));
        if (timeBlock.Trim().Length > 100)
            throw new ArgumentException("Time block cannot exceed 100 characters.", nameof(timeBlock));
        if (string.IsNullOrWhiteSpace(confirmationStatus))
            throw new ArgumentException("Confirmation status is required.", nameof(confirmationStatus));
        if (callNotes?.Length > 4000)
            throw new ArgumentException("Call notes cannot exceed 4000 characters.", nameof(callNotes));
        if (notesFlag?.Length > 4000)
            throw new ArgumentException("Notes / flag cannot exceed 4000 characters.", nameof(notesFlag));

        // Parse before mutating the entity so an invalid time block cannot leave partial state behind.
        var normalizedTimeBlock = timeBlock.Trim().Replace('–', '-');
        var timeParts = normalizedTimeBlock.Split('-', 2, StringSplitOptions.TrimEntries);
        if (timeParts.Length == 2 && !DateTime.TryParse(
                timeParts[1],
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                out _))
            throw new ArgumentException("Enter a valid end time in the time block.", nameof(timeBlock));
        var startTime = ParseStartTime(normalizedTimeBlock);
        var normalizedStatus = new[] { "Scheduled", "Completed", "Missed", "Rescheduled", "Cancelled" }
            .FirstOrDefault(x => string.Equals(x, status?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedStatus is null)
            throw new ArgumentException("Invalid visit status.", nameof(status));

        TimeBlock = timeBlock.Trim();
        ScheduledDate = ScheduledDate.Date.Add(startTime);
        ConfirmationStatus = confirmationStatus.Trim();
        CallNotes = string.IsNullOrWhiteSpace(callNotes) ? null : callNotes.Trim();
        ClinicianId = clinicianId;
        ClinicianName = string.IsNullOrWhiteSpace(clinicianName) ? null : clinicianName.Trim();
        NotesFlag = string.IsNullOrWhiteSpace(notesFlag) ? null : notesFlag.Trim();
        ChangeVisitStatus(normalizedStatus);
    }

    public void AssignCareCoordinator(Guid careCoordinatorId)
    {
        if (careCoordinatorId == Guid.Empty) throw new ArgumentException("Select a Care Coordinator.");
        if (CareCoordinatorId.HasValue) throw new InvalidOperationException("This visit already has a Care Coordinator.");
        CareCoordinatorId = careCoordinatorId;
    }

    public void ChangeVisitStatus(string status)
    {
        var normalized = new[] { "Scheduled", "Completed", "Missed", "Rescheduled", "Cancelled" }
            .FirstOrDefault(x => string.Equals(x, status?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalized is null)
            throw new ArgumentException("Invalid visit status.", nameof(status));

        Status = normalized;
        CompletedDate = normalized == "Completed"
            ? CompletedDate ?? DateTime.UtcNow
            : null;
    }

    public void Reschedule(DateTime scheduledDate)
    {
        if (string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A completed visit cannot be rescheduled.");

        ScheduledDate = scheduledDate;
        Status = "Scheduled";
    }

    public void UpdateImportedDetails(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void Complete(string? notes)
    {
        if (string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase))
            return;

        Status = "Completed";
        CompletedDate = DateTime.UtcNow;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
