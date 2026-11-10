namespace CCAP.Domain.Entities;

public sealed class PatientCareLog
{
    private PatientCareLog() { }

    public Guid PatientCareLogId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? RecordedByUserId { get; private set; }
    public string LogType { get; private set; } = string.Empty;
    public string Item { get; private set; } = string.Empty;
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public string? Notes { get; private set; }
    public string? Status { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public ApplicationUser? RecordedByUser { get; private set; }

    public PatientCareLog(Guid patientId, Guid? userId, string logType, string item,
        decimal? quantity, string? unit, string? notes, string? status = null)
    {
        if (patientId == Guid.Empty) throw new ArgumentException("Patient ID is required.", nameof(patientId));
        if (string.IsNullOrWhiteSpace(logType)) throw new ArgumentException("Log type is required.", nameof(logType));
        if (string.IsNullOrWhiteSpace(item)) throw new ArgumentException("Item is required.", nameof(item));

        PatientCareLogId = Guid.NewGuid();
        PatientId = patientId;
        RecordedByUserId = userId;
        LogType = logType.Trim();
        Item = item.Trim();
        Quantity = quantity;
        Unit = Normalize(unit);
        Notes = Normalize(notes);
        Status = Normalize(status);
        RecordedAt = DateTime.UtcNow;
    }

    public void Update(string item, decimal? quantity, string? unit, string? notes, string? status)
    {
        if (string.IsNullOrWhiteSpace(item)) throw new ArgumentException("Item is required.", nameof(item));
        if (quantity < 0) throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));

        Item = item.Trim();
        Quantity = quantity;
        Unit = Normalize(unit);
        Notes = Normalize(notes);
        Status = Normalize(status);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
