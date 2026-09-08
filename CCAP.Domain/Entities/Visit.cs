namespace CCAP.Domain.Entities;

public sealed class Visit
{
    private Visit()
    {
    }

    public Guid VisitId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid ClinicianId { get; private set; }

    public DateTime ScheduledDate { get; private set; }

    public DateTime? CompletedDate { get; private set; }

    public string Status { get; private set; } = "Scheduled";

    public string? Notes { get; private set; }

    public Patient Patient { get; private set; } = null!;

    public ApplicationUser Clinician { get; private set; } = null!;


    public Visit(
        Guid patientId,
        Guid clinicianId,
        DateTime scheduledDate)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Patient ID is required.",
                nameof(patientId));
        }

        if (clinicianId == Guid.Empty)
        {
            throw new ArgumentException(
                "Clinician ID is required.",
                nameof(clinicianId));
        }

        VisitId = Guid.NewGuid();

        PatientId = patientId;

        ClinicianId = clinicianId;

        ScheduledDate = scheduledDate;

        Status = "Scheduled";
    }


    public void Reschedule(
        DateTime scheduledDate)
    {
        if (string.Equals(
            Status,
            "Completed",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A completed visit cannot be rescheduled.");
        }

        ScheduledDate = scheduledDate;

        Status = "Scheduled";
    }


    public void Complete(
        string? notes)
    {
        if (string.Equals(
            Status,
            "Completed",
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Status = "Completed";

        CompletedDate = DateTime.UtcNow;

        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }
}