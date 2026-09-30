using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.CompleteSoc;

public sealed class CompleteSocCommandValidator : IRequestValidator<CompleteSocCommand>
{
    public void Validate(CompleteSocCommand request)
    {
        if (request.PatientId == Guid.Empty) throw new ArgumentException("Patient ID is required.");
        if (request.CompletedByUserId == Guid.Empty) throw new ArgumentException("Completed by user ID is required.");
        if (request.Notes is not null && request.Notes.Length > 5000) throw new ArgumentException("SOC notes cannot exceed 5000 characters.");
    }
}
