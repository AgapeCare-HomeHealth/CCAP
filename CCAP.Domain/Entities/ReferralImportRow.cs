namespace CCAP.Domain.Entities;

public sealed class ReferralImportRow
{
    public Guid ReferralImportRowId { get; private set; } = Guid.NewGuid();
    public Guid ReferralImportBatchId { get; private set; }
    public int RowNumber { get; private set; }
    public string PatientName { get; private set; } = "";
    public string Status { get; private set; } = "";
    public string? ErrorMessage { get; private set; }
    public string RawDataJson { get; private set; } = "{}";

    private ReferralImportRow() { }
    public ReferralImportRow(Guid batchId, int rowNumber, string patientName, string status, string? error, string rawDataJson)
    {
        ReferralImportBatchId = batchId;
        RowNumber = rowNumber;
        PatientName = patientName;
        Status = status;
        ErrorMessage = error;
        RawDataJson = rawDataJson;
    }
}
