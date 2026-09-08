using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteSocCompliance;

public sealed class CompleteSocComplianceCommandHandler
    : IRequestHandler<CompleteSocComplianceCommand>
{
    private const string SocCompliantRequirement =
        "SOC_COMPLIANT";

    private readonly IPatientRepository _patients;
    private readonly IComplianceRepository _compliance;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteSocComplianceCommandHandler(
        IPatientRepository patients,
        IComplianceRepository compliance,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _compliance = compliance;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteSocComplianceCommand request,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(
            request.PatientId,
            cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }

        var complianceRecord =
            await _compliance.GetByPatientAndRequirementAsync(
                request.PatientId,
                SocCompliantRequirement,
                cancellationToken);

        if (complianceRecord is null)
        {
            throw new KeyNotFoundException(
                "SOC Compliant requirement was not found.");
        }

        if (complianceRecord.IsCompleted)
        {
            return;
        }

        complianceRecord.Complete(
            request.VerifiedByUserId);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}