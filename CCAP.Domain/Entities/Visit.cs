namespace CCAP.Domain.Entities;

public sealed class Visit
{
    private Visit() { }

    public Guid VisitId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ClinicianId { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string Status { get; private set; } = "Scheduled";
    public string? Notes { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public ApplicationUser Clinician { get; private set; } = null!;

    public Visit(Guid patientId, Guid clinicianId, DateTime scheduledDate)
    {
        VisitId = Guid.NewGuid();
        PatientId = patientId;
        ClinicianId = clinicianId;
        ScheduledDate = scheduledDate;
    }

    public void Complete(string? notes)
    {
        Status = "Completed";
        CompletedDate = DateTime.UtcNow;
        Notes = notes;
    }
}
