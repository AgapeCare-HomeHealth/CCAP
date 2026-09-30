namespace CCAP.Domain.Entities;

public sealed class PatientServiceOrder
{
    private PatientServiceOrder() { }

    public Guid PatientServiceOrderId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ServiceTypeId { get; private set; }
    public string Status { get; private set; } = "Ordered";
    public string? Frequency { get; private set; }
    public string? Duration { get; private set; }
    public bool IsPrimaryDiscipline { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public ServiceType ServiceType { get; private set; } = null!;

    public PatientServiceOrder(
        Guid patientId,
        Guid serviceTypeId,
        string? frequency,
        string? duration,
        bool isPrimaryDiscipline)
    {
        PatientServiceOrderId = Guid.NewGuid();
        PatientId = patientId;
        ServiceTypeId = serviceTypeId;
        Frequency = frequency;
        Duration = duration;
        IsPrimaryDiscipline = isPrimaryDiscipline;
    }

    public void Update(string status, string? frequency, string? duration, bool primary)
    {
        Status = status;
        Frequency = frequency;
        Duration = duration;
        IsPrimaryDiscipline = primary;
    }
}
