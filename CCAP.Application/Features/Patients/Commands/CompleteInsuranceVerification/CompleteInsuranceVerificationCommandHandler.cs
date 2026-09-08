using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteInsuranceVerification;

public sealed class CompleteInsuranceVerificationCommandHandler
    : IRequestHandler<CompleteInsuranceVerificationCommand>
{
    private const string InsuranceVerificationRequirement =
        "INSURANCE_VERIFICATION";

    private readonly IPatientRepository _patients;
    private readonly IComplianceRepository _compliance;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteInsuranceVerificationCommandHandler(
        IPatientRepository patients,
        IComplianceRepository compliance,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _compliance = compliance;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteInsuranceVerificationCommand request,
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
                InsuranceVerificationRequirement,
                cancellationToken);

        if (complianceRecord is null)
        {
            throw new KeyNotFoundException(
                "Insurance verification requirement was not found.");
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