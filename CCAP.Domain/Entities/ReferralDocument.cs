namespace CCAP.Domain.Entities;

public sealed class ReferralDocument
{
    private ReferralDocument()
    {
    }

    public Guid ReferralDocumentId { get; private set; }

    public Guid ReferralId { get; private set; }

    public string StorageKey { get; private set; } = string.Empty;

    public string OriginalFileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long FileSize { get; private set; }

    public DateTime UploadedAt { get; private set; }

    public Referral Referral { get; private set; } = null!;

    public ReferralDocument(
        Guid referralId,
        string storageKey,
        string originalFileName,
        string contentType,
        long fileSize)
    {
        if (referralId == Guid.Empty)
            throw new ArgumentException(
                "Referral ID is required.",
                nameof(referralId));

        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException(
                "Storage key is required.",
                nameof(storageKey));

        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException(
                "Original file name is required.",
                nameof(originalFileName));

        if (fileSize < 0)
            throw new ArgumentOutOfRangeException(
                nameof(fileSize));

        ReferralDocumentId = Guid.NewGuid();

        ReferralId = referralId;

        StorageKey = storageKey.Trim();

        OriginalFileName = originalFileName.Trim();

        ContentType =
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType.Trim();

        FileSize = fileSize;

        UploadedAt = DateTime.UtcNow;
    }
}