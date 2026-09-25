using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.UploadPreAuthDocument;

public sealed class UploadPreAuthDocumentValidator : IRequestValidator<UploadPreAuthDocumentCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/jpg"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg"
    };

    public void Validate(UploadPreAuthDocumentCommand request)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.");

        if (request.UploadedByUserId == Guid.Empty)
            throw new ArgumentException("Uploaded by user ID is required.");

        if (request.Content is null)
            throw new ArgumentException("File content is required.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException("File name is required.");

        if (request.FileSize <= 0)
            throw new ArgumentException("The selected file is empty.");

        if (request.FileSize > MaxFileSize)
            throw new ArgumentException("The insurance document must be 10 MB or smaller.");

        var extension = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only PDF, JPG, JPEG, and PNG files are supported.");

        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new ArgumentException("Only PDF, JPG, JPEG, and PNG files are supported.");
    }
}
