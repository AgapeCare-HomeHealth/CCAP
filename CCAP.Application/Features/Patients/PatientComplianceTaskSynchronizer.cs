using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using CCAP.Domain.Enums;

namespace CCAP.Application.Features.Patients;

/// <summary>
/// Creates one task per actionable compliance requirement and synchronizes task
/// state from the persisted/tracked compliance records.
/// </summary>
public static class PatientComplianceTaskSynchronizer
{
    public static async Task EnsureAllAsync(
        Patient patient,
        IPatientTaskRepository tasks,
        IComplianceRepository compliance,
        CancellationToken ct)
    {
        var records = await compliance.GetByPatientAsync(patient.PatientId, ct);

        foreach (var item in PatientComplianceTaskCatalog.Items)
            await EnsureAsync(patient, item.Key, tasks, records, ct);

        // Retire legacy tasks incorrectly generated from informational fields.
        var informationalTaskTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Compliance: Pre-Auth Due", "Compliance: Number of Visits", "Compliance: Hospital DC Summary",
            "Compliance: SOC Feedback from Patient", "Compliance: Case Mix Type", "Compliance: TIF",
            "Compliance: ROC", "Compliance: Recertification Received", "Compliance: Discharge Date",
            "Compliance: DC Feedback From Patient"
        };
        var patientTasks = await tasks.GetByPatientAsync(patient.PatientId, ct);
        foreach (var staleTask in patientTasks.Where(x => informationalTaskTitles.Contains(x.Title)
            && x.Status != PatientTaskStatus.Completed && x.Status != PatientTaskStatus.Cancelled))
            staleTask.Cancel();

        // Insurance is a separate workflow stage and uses the same record-driven state rule.
        var insuranceTask = await tasks.GetByPatientAndTitleAsync(patient.PatientId, "Verify Insurance", ct);
        if (insuranceTask is null)
        {
            insuranceTask = new PatientTask(
                patient.PatientId,
                "Verify Insurance",
                "Review insurance eligibility and authorization, then complete Insurance Verification.",
                DateTime.UtcNow.AddDays(1),
                $"/tracker/patient/{patient.PatientId}");
            if (patient.CoordinatorId.HasValue)
                insuranceTask.Assign(patient.CoordinatorId.Value);
            await tasks.AddAsync(insuranceTask, ct);
        }
        else if (!insuranceTask.AssignedUserId.HasValue && patient.CoordinatorId.HasValue)
        {
            insuranceTask.Assign(patient.CoordinatorId.Value);
        }

        var insuranceRecord = records.FirstOrDefault(x =>
            string.Equals(x.RequirementCode, "INSURANCE_VERIFICATION", StringComparison.OrdinalIgnoreCase));
        var insuranceComplete = patient.InsuranceVerifiedAt.HasValue || insuranceRecord?.IsCompleted == true;
        SyncState(insuranceTask, insuranceComplete);
    }

    public static async Task EnsureAsync(
        Patient patient,
        string code,
        IPatientTaskRepository tasks,
        IComplianceRepository compliance,
        CancellationToken ct)
    {
        if (!PatientComplianceTaskCatalog.Items.TryGetValue(code, out _))
            return;

        var records = await compliance.GetByPatientAsync(patient.PatientId, ct);
        await EnsureAsync(patient, code, tasks, records, ct);
    }

    private static async Task EnsureAsync(
        Patient patient,
        string code,
        IPatientTaskRepository tasks,
        IReadOnlyCollection<ComplianceRecord> records,
        CancellationToken ct)
    {
        if (!PatientComplianceTaskCatalog.Items.TryGetValue(code, out var item))
            return;

        var title = PatientComplianceTaskCatalog.TaskTitle(code);
        var task = await tasks.GetByPatientAndTitleAsync(patient.PatientId, title, ct);

        if (task is null)
        {
            task = new PatientTask(
                patient.PatientId,
                title,
                item.Description,
                DateTime.UtcNow.AddDays(1),
                $"/tracker/patient/{patient.PatientId}");
            if (patient.CoordinatorId.HasValue)
                task.Assign(patient.CoordinatorId.Value);
            await tasks.AddAsync(task, ct);
        }
        else if (patient.CoordinatorId.HasValue &&
                 (!task.AssignedUserId.HasValue || task.AssignedUserId == patient.CoordinatorId))
        {
            // Keep default ownership aligned with the Primary CC, but do not
            // overwrite an explicit administrator reassignment to another user.
            task.Assign(patient.CoordinatorId.Value);
        }

        var record = records.FirstOrDefault(x =>
            string.Equals(x.RequirementCode, code, StringComparison.OrdinalIgnoreCase));
        SyncState(task, record?.IsCompleted == true);
    }

    private static void SyncState(PatientTask task, bool isComplete)
    {
        if (isComplete && task.Status != PatientTaskStatus.Completed &&
            task.Status != PatientTaskStatus.Cancelled)
            task.Complete();
        else if (!isComplete && task.Status == PatientTaskStatus.Completed)
            task.Reopen();
    }
}
