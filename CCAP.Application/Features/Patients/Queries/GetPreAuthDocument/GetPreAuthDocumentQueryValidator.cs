using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Queries.GetPreAuthDocument;

public sealed class GetPreAuthDocumentQueryValidator : IRequestValidator<GetPreAuthDocumentQuery>
{
    public void Validate(GetPreAuthDocumentQuery request)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.");
    }
}
