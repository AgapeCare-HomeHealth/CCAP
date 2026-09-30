using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteInsuranceVerification;

public sealed class CompleteInsuranceVerificationCommandHandler : IRequestHandler<CompleteInsuranceVerificationCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IPatientTaskRepository _tasks;
    private readonly IComplianceRepository _compliance;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientAuditLogRepository _auditLogs;

    public CompleteInsuranceVerificationCommandHandler(IPatientRepository patients, IPatientTaskRepository tasks, IComplianceRepository compliance, IUnitOfWork unitOfWork, IPatientAuditLogRepository auditLogs)
    { _patients = patients; _tasks = tasks; _compliance = compliance; _unitOfWork = unitOfWork; _auditLogs = auditLogs; }

    public async Task Handle(CompleteInsuranceVerificationCommand request, CancellationToken cancellationToken)
    {
        if (request.PatientId == Guid.Empty) throw new ArgumentException("Patient ID is required.");
        if (request.VerifiedByUserId == Guid.Empty) throw new ArgumentException("Verified by user ID is required.");

        var patient = await _patients.GetByIdForUpdateAsync(request.PatientId, cancellationToken) ?? throw new KeyNotFoundException("Patient not found.");
        if (patient.InsuranceVerifiedAt.HasValue) return;

        patient.VerifyInsurance(request.VerifiedByUserId);

        var compliance = await _compliance.GetByPatientAndRequirementAsync(request.PatientId, "INSURANCE_VERIFICATION", cancellationToken);
        if (compliance is null)
        {
            compliance = new ComplianceRecord(request.PatientId, "INSURANCE_VERIFICATION", "Insurance eligibility and authorization were reviewed and confirmed.");
            await _compliance.AddAsync(compliance, cancellationToken);
        }
        if (!compliance.IsCompleted) compliance.Complete(request.VerifiedByUserId);

        var task = await _tasks.GetPendingByPatientAndTitleAsync(request.PatientId, "Verify Insurance", cancellationToken);
        task?.Complete();

        patient.Activities.Add(new Activity(patient.PatientId, request.VerifiedByUserId, "Insurance", "Insurance verification completed", "Insurance was verified."));
        await _auditLogs.AddAsync(new PatientAuditLog(patient.PatientId, request.VerifiedByUserId, "Insurance", patient.PatientId.ToString(), "UPDATE", "Unverified", "Verified", "Insurance verification completed."), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
