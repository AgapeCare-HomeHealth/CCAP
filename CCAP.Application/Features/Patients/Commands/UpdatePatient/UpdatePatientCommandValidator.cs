using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.UpdatePatient;

public sealed class UpdatePatientCommandValidator : IRequestValidator<UpdatePatientCommand>
{
    public void Validate(UpdatePatientCommand request)
    {
        if (request.PatientId == Guid.Empty) throw new ArgumentException("Patient ID is required.");
        if (string.IsNullOrWhiteSpace(request.MRN)) throw new ArgumentException("MRN is required.");
        if (request.MRN.Length > 50) throw new ArgumentException("MRN cannot exceed 50 characters.");
        if (string.IsNullOrWhiteSpace(request.FirstName)) throw new ArgumentException("First name is required.");
        if (request.FirstName.Length > 100) throw new ArgumentException("First name cannot exceed 100 characters.");
        if (request.MiddleName is not null && request.MiddleName.Length > 100) throw new ArgumentException("Middle name cannot exceed 100 characters.");
        if (string.IsNullOrWhiteSpace(request.LastName)) throw new ArgumentException("Last name is required.");
        if (request.LastName.Length > 100) throw new ArgumentException("Last name cannot exceed 100 characters.");
    }
}
