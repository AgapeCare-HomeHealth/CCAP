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

    public string PatientName { get; private set; } = string.Empty;
    public string? ClinicianName { get; private set; }
    public string VisitType { get; private set; } = "Visit";
    public string? TimeBlock { get; private set; }
    public string? ConfirmationStatus { get; private set; }
    public string? CallNotes { get; private set; }
    public string? NotesFlag { get; private set; }
    public string? Location { get; private set; }

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
        Guid assignedUserId)
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

        return new Visit
        {
            VisitId = Guid.NewGuid(),
            PatientName = patientName.Trim(),
            ClinicianId = clinicianId,
            AssignedUserId = assignedUserId,
            ScheduledDate = scheduledDate,
            TimeBlock = timeBlock.Trim(),
            ConfirmationStatus = confirmationStatus.Trim(),
            CallNotes = string.IsNullOrWhiteSpace(callNotes) ? null : callNotes.Trim(),
            VisitType = visitType.Trim(),
            Status = string.IsNullOrWhiteSpace(visitStatus) ? "Scheduled" : visitStatus.Trim(),
            NotesFlag = string.IsNullOrWhiteSpace(notesFlag) ? null : notesFlag.Trim()
        };
    }

    // Used for imported external schedules. No Patient entity is required.
    public static Visit CreateImportedSchedule(
        string patientName,
        DateTime scheduledDate,
        string? clinicianName,
        Guid? patientId,
        Guid? clinicianId,
        Guid assignedUserId,
        string visitType = "Visit",
        string? location = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name is required.", nameof(patientName));

        if (assignedUserId == Guid.Empty)
            throw new ArgumentException("Assigned user ID is required.", nameof(assignedUserId));

        var visit = new Visit
        {
            VisitId = Guid.NewGuid(),
            PatientId = patientId,
            ClinicianId = clinicianId,
            AssignedUserId = assignedUserId,
            PatientName = patientName.Trim(),
            ClinicianName = string.IsNullOrWhiteSpace(clinicianName) ? null : clinicianName.Trim(),
            VisitType = string.IsNullOrWhiteSpace(visitType) ? "Visit" : visitType.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            ScheduledDate = scheduledDate,
            Status = "Scheduled",
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        return visit;
    }

    public void Reschedule(DateTime scheduledDate)
    {
        if (string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A completed visit cannot be rescheduled.");

        ScheduledDate = scheduledDate;
        Status = "Scheduled";
    }

    public void UpdateImportedSchedule(
        string patientName,
        DateTime scheduledDate,
        string? clinicianName,
        Guid? patientId,
        Guid? clinicianId,
        Guid? assignedUserId,
        string visitType,
        string? location,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name is required.", nameof(patientName));

        PatientName = patientName.Trim();
        ScheduledDate = scheduledDate;
        ClinicianName = string.IsNullOrWhiteSpace(clinicianName) ? null : clinicianName.Trim();
        PatientId = patientId;
        ClinicianId = clinicianId;
        AssignedUserId = assignedUserId;
        VisitType = string.IsNullOrWhiteSpace(visitType) ? "Visit" : visitType.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
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
