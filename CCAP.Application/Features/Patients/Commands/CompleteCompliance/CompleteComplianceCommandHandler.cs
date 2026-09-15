using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Enums;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteCompliance;

public sealed class CompleteComplianceCommandHandler
    : IRequestHandler<CompleteComplianceCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IComplianceRepository _compliance;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientTaskRepository _tasks;
    private readonly IPatientAuditLogRepository _auditLogs;

    public CompleteComplianceCommandHandler(
        IPatientRepository patients,
        IComplianceRepository compliance,
        IUnitOfWork unitOfWork, IPatientAuditLogRepository auditLogs, IPatientTaskRepository tasks)
    {
        _patients = patients;
        _compliance = compliance;
        _unitOfWork = unitOfWork;
        _auditLogs = auditLogs;
        _tasks = tasks;
    }

    public async Task Handle(
        CompleteComplianceCommand request,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdForUpdateAsync(
            request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        var requirementCode = request.RequirementCode.Trim().ToUpperInvariant();

        var compliance = await _compliance.GetByPatientAndRequirementAsync(
            request.PatientId, requirementCode, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Compliance requirement '{requirementCode}' was not found.");

        if (compliance.IsCompleted)
            return;

        if (string.Equals(requirementCode, "INSURANCE_VERIFICATION", StringComparison.OrdinalIgnoreCase) && !patient.InsuranceVerifiedAt.HasValue)
            patient.VerifyInsurance(request.CompletedByUserId);

        compliance.Complete(request.CompletedByUserId);
        patient.Activities.Add(new CCAP.Domain.Entities.Activity(patient.PatientId, request.CompletedByUserId, "Compliance", "Compliance completed", $"{requirementCode} marked completed."));
        await _auditLogs.AddAsync(new CCAP.Domain.Entities.PatientAuditLog(patient.PatientId, request.CompletedByUserId, "Compliance", compliance.ComplianceRecordId.ToString(), "UPDATE", "Pending", "Completed", $"{requirementCode} completed."), cancellationToken);

        var taskTitle = GetMatchingTaskTitle(requirementCode);
        if (!string.IsNullOrWhiteSpace(taskTitle))
        {
            var task = await _tasks.GetPendingByPatientAndTitleAsync(request.PatientId, taskTitle, cancellationToken);
            task?.Complete();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? GetMatchingTaskTitle(string requirementCode)
    {
        return requirementCode switch
        {
            "INSURANCE_VERIFICATION" => "Verify Insurance",
            "PHYSICIAN_ORDERS" or "ORDERS_SIGNED" => "Verify Physician Orders",
            _ => null
        };
    }
}
