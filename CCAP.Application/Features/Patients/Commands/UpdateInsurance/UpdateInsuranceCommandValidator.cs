using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.UpdateInsurance;

public sealed class UpdateInsuranceCommandValidator : IRequestValidator<UpdateInsuranceCommand>
{
    public void Validate(UpdateInsuranceCommand request)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.");
        if (string.IsNullOrWhiteSpace(request.PrimaryInsurance))
            throw new ArgumentException("Primary insurance is required.");
        if (request.PrimaryInsurance.Length > 200)
            throw new ArgumentException("Primary insurance cannot exceed 200 characters.");
        if (request.InsuranceMemberId is not null && request.InsuranceMemberId.Length > 100)
            throw new ArgumentException("Insurance member ID cannot exceed 100 characters.");
        if (request.AuthorizationDate.HasValue && request.AuthorizationDate.Value > DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new ArgumentException("Authorization date cannot be in the future.");
        if (request.ApprovedVisits.HasValue && request.ApprovedVisits.Value < 0)
            throw new ArgumentException("Approved visits cannot be negative.");
    }
}
