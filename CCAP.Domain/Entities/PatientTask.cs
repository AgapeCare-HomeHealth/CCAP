using CCAP.Domain.Enums;

namespace CCAP.Domain.Entities;

public sealed class PatientTask
{
    private PatientTask() { }

    public Guid TaskId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime DueDate { get; private set; }
    public PatientTaskStatus Status { get; private set; }
    public string? PageRoute { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public ApplicationUser? AssignedUser { get; private set; }

    public PatientTask(Guid patientId, string title, string description, DateTime dueDate, string? pageRoute)
    {
        TaskId = Guid.NewGuid();
        PatientId = patientId;
        Title = title;
        Description = description;
        DueDate = dueDate;
        PageRoute = pageRoute;
        Status = PatientTaskStatus.Pending;
    }

    public void Assign(Guid userId) => AssignedUserId = userId;
    public void Start()
    {
        if (Status == PatientTaskStatus.Cancelled)
            throw new InvalidOperationException("A cancelled task cannot be started.");

        Status = PatientTaskStatus.InProgress;
    }

    public void Complete()
    {
        if (Status == PatientTaskStatus.Cancelled)
            throw new InvalidOperationException("A cancelled task cannot be completed.");

        Status = PatientTaskStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == PatientTaskStatus.Completed)
            throw new InvalidOperationException("A completed task cannot be cancelled.");

        Status = PatientTaskStatus.Cancelled;
    }
}
