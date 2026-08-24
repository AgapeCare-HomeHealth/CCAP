using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.AddCallNote;

public sealed class AddCallNoteCommandValidator
    : IRequestValidator<AddCallNoteCommand>
{
    public void Validate(
        AddCallNoteCommand request)
    {
        if (request.PatientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Patient ID is required.");
        }

        if (request.RecordedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Recorded by user ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new ArgumentException(
                "Call note subject is required.");
        }

        if (request.Subject.Length > 200)
        {
            throw new ArgumentException(
                "Call note subject cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            throw new ArgumentException(
                "Call note content is required.");
        }

        if (request.Notes.Length > 5000)
        {
            throw new ArgumentException(
                "Call note content cannot exceed 5000 characters.");
        }

        if (request.Outcome is not null &&
            request.Outcome.Length > 500)
        {
            throw new ArgumentException(
                "Call note outcome cannot exceed 500 characters.");
        }
    }
}