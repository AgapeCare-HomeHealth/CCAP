using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.UpdateInsurance;

public sealed class UpdateInsuranceCommandHandler
    : IRequestHandler<UpdateInsuranceCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInsuranceCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateInsuranceCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await _patients.GetByIdForUpdateAsync(
                request.PatientId,
                cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }

        var normalizedInsurance =
            Normalize(request.PrimaryInsurance);

        var normalizedMemberId =
            Normalize(request.InsuranceMemberId);

        var insuranceChanged =
            !string.Equals(
                patient.PrimaryInsurance,
                normalizedInsurance,
                StringComparison.Ordinal)
            ||
            !string.Equals(
                patient.InsuranceMemberId,
                normalizedMemberId,
                StringComparison.Ordinal)
            ||
            patient.AuthorizationDate !=
                request.AuthorizationDate
            ||
            patient.ApprovedVisits !=
                request.ApprovedVisits
            ||
            patient.AuthorizationRequired !=
                request.AuthorizationRequired;

        patient.UpdateInsurance(
            request.PrimaryInsurance,
            request.InsuranceMemberId,
            request.AuthorizationDate,
            request.ApprovedVisits,
            request.AuthorizationRequired);

        if (insuranceChanged &&
            patient.InsuranceVerifiedAt.HasValue)
        {
            patient.RequireInsuranceReverification();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}