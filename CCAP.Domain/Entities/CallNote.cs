namespace CCAP.Domain.Entities;

public sealed class CallNote
{
    private CallNote() { }

    public Guid CallNoteId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime CallDate { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public string? Outcome { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public ApplicationUser RecordedBy { get; private set; } = null!;

    public CallNote(Guid patientId, Guid recordedByUserId, string subject, string notes, string? outcome)
    {
        CallNoteId = Guid.NewGuid();
        PatientId = patientId;
        RecordedByUserId = recordedByUserId;
        CallDate = DateTime.UtcNow;
        Subject = subject.Trim();
        Notes = notes.Trim();
        Outcome = outcome?.Trim();
    }
}
