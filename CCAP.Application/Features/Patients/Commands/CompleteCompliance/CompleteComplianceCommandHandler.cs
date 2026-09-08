using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteCompliance;

public sealed class CompleteComplianceCommandHandler
    : IRequestHandler<CompleteComplianceCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IComplianceRepository _compliance;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteComplianceCommandHandler(
        IPatientRepository patients,
        IComplianceRepository compliance,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _compliance = compliance;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteComplianceCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await _patients.GetByIdAsync(
                request.PatientId,
                cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }

        var requirementCode =
            request.RequirementCode.Trim()
                .ToUpperInvariant();

        var complianceRecord =
            await _compliance
                .GetByPatientAndRequirementAsync(
                    request.PatientId,
                    requirementCode,
                    cancellationToken);

        if (complianceRecord is null)
        {
            throw new KeyNotFoundException(
                $"Compliance requirement '{requirementCode}' was not found.");
        }

        if (complianceRecord.IsCompleted)
        {
            return;
        }

        complianceRecord.Complete(
            request.CompletedByUserId);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}