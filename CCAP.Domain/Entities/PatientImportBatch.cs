namespace CCAP.Domain.Entities;

public sealed class PatientImportBatch
{
    private PatientImportBatch() { }
    public Guid PatientImportBatchId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileFingerprint { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Preview";
    public int TotalRows { get; private set; }
    public int InsertedRows { get; private set; }
    public int DuplicateRows { get; private set; }
    public int FailedRows { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Summary { get; private set; }

    public PatientImportBatch(string fileName, string fileFingerprint, int totalRows, Guid uploadedByUserId)
    {
        PatientImportBatchId = Guid.NewGuid();
        FileName = fileName;
        FileFingerprint = fileFingerprint;
        TotalRows = totalRows;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = DateTime.UtcNow;
    }

    public void Queue() { if (Status != "Preview") throw new InvalidOperationException("Only a preview can be queued."); Status = "Queued"; }
    public void Start() { Status = "Processing"; StartedAt ??= DateTime.UtcNow; }
    public void RefreshCounts(int inserted, int duplicates, int failed, int total)
    {
        InsertedRows = inserted; DuplicateRows = duplicates; FailedRows = failed; TotalRows = total;
        if (Status == "Processing" && inserted + duplicates + failed >= total)
        {
            Status = failed > 0 ? (inserted > 0 ? "Partial" : "Failed") : "Completed";
            CompletedAt = DateTime.UtcNow;
        }
    }
    public void SetSummary(string? summary) => Summary = summary;
}
