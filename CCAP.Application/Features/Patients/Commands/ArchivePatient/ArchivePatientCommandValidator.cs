using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.ArchivePatient;

public sealed class ArchivePatientCommandValidator
    : IRequestValidator<ArchivePatientCommand>
{
    public void Validate(
        ArchivePatientCommand request)
    {
        if (request.PatientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Patient ID is required.");
        }

        if (request.ArchivedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Archived by user ID is required.");
        }
    }
}