namespace CCAP.Domain.Entities;

public sealed class PatientComplianceDocument
{
    private PatientComplianceDocument()
    {
    }

    public Guid PatientComplianceDocumentId { get; private set; }
    public Guid PatientId { get; private set; }
    public string RequirementCode { get; private set; } = string.Empty;
    public string? StorageKey { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }

    public Patient Patient { get; private set; } = null!;

    public PatientComplianceDocument(
        Guid patientId,
        string requirementCode,
        string? storageKey,
        string originalFileName,
        string contentType,
        long fileSize,
        Guid uploadedByUserId)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.", nameof(patientId));

        if (string.IsNullOrWhiteSpace(requirementCode))
            throw new ArgumentException("Requirement code is required.", nameof(requirementCode));

        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));

        if (fileSize < 0)
            throw new ArgumentOutOfRangeException(nameof(fileSize));

        if (uploadedByUserId == Guid.Empty)
            throw new ArgumentException("Uploaded by user ID is required.", nameof(uploadedByUserId));

        PatientComplianceDocumentId = Guid.NewGuid();
        PatientId = patientId;
        RequirementCode = requirementCode.Trim();
        StorageKey = string.IsNullOrWhiteSpace(storageKey) ? null : storageKey.Trim();
        OriginalFileName = originalFileName.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim();
        FileSize = fileSize;
        UploadedAt = DateTime.UtcNow;
        UploadedByUserId = uploadedByUserId;
    }
}
