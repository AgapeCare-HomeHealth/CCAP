using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocumentFile;

public sealed class GetPreAuthDocumentFileQueryValidator : IRequestValidator<GetPreAuthDocumentFileQuery>
{
    public void Validate(GetPreAuthDocumentFileQuery request)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.");
    }
}
