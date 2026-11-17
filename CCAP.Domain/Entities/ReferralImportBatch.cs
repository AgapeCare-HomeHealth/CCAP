namespace CCAP.Domain.Entities;

public sealed class ReferralImportBatch
{
    public Guid ReferralImportBatchId { get; private set; } = Guid.NewGuid();
    public string FileName { get; private set; } = "";
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;
    public int TotalRows { get; private set; }
    public int InsertedRows { get; private set; }
    public int DuplicateRows { get; private set; }
    public int FailedRows { get; private set; }
    public string Status { get; private set; } = "Completed";

    private ReferralImportBatch() { }
    public ReferralImportBatch(string fileName, Guid uploadedBy, int total, int inserted, int duplicates, int failed)
    {
        FileName = fileName;
        UploadedByUserId = uploadedBy;
        TotalRows = total;
        InsertedRows = inserted;
        DuplicateRows = duplicates;
        FailedRows = failed;
        Status = failed > 0 ? (inserted > 0 ? "Partial" : "Failed") : "Completed";
    }
}
