namespace CCAP.Domain.Entities;

public sealed class PatientImportRow
{
    private PatientImportRow() { }
    public Guid PatientImportRowId { get; private set; }
    public Guid PatientImportBatchId { get; private set; }
    public int RowNumber { get; private set; }
    public string RecordFingerprint { get; private set; } = string.Empty;
    public string RawDataJson { get; private set; } = "{}";
    public string Status { get; private set; } = "Ready";
    public string? ErrorMessage { get; private set; }
    public string? GeneratedMrn { get; private set; }
    public string? GeneratedReferralNumber { get; private set; }
    public Guid? PatientId { get; private set; }
    public Guid? ReferralId { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public PatientImportRow(Guid batchId, int rowNumber, string fingerprint, string rawDataJson, string? errorMessage = null, string? status = null)
    {
        PatientImportRowId = Guid.NewGuid(); PatientImportBatchId = batchId; RowNumber = rowNumber;
        RecordFingerprint = fingerprint; RawDataJson = rawDataJson; ErrorMessage = errorMessage;
        Status = status ?? (string.IsNullOrWhiteSpace(errorMessage) ? "Ready" : "Failed");
        GeneratedMrn = $"MRN-IMP-{Guid.NewGuid():N}";
        GeneratedReferralNumber = $"REF-IMP-{Guid.NewGuid():N}";
    }
    public void UseIdentifiers(string? mrn, string? referralNumber) { if (!string.IsNullOrWhiteSpace(mrn)) GeneratedMrn = mrn; if (!string.IsNullOrWhiteSpace(referralNumber)) GeneratedReferralNumber = referralNumber; }
    public void UseCreatedRecords(Guid? patientId, Guid? referralId) { PatientId = patientId; ReferralId = referralId; }
    public void AttachCreatedRecords(Guid patientId, Guid referralId) { PatientId = patientId; ReferralId = referralId; }
    public void MarkDuplicate(string reason) { Status = "Duplicate"; ErrorMessage = reason; ProcessedAt = DateTime.UtcNow; }
    public void MarkProcessing() { Status = "Processing"; AttemptCount++; ErrorMessage = null; }
    public void MarkSucceeded(Guid patientId, Guid referralId) { PatientId = patientId; ReferralId = referralId; Status = "Inserted"; ErrorMessage = null; ProcessedAt = DateTime.UtcNow; }
    public void MarkFailed(string error) { Status = "Failed"; ErrorMessage = error; ProcessedAt = DateTime.UtcNow; }
    public void ResetForRetry() { if (Status == "Inserted" || Status == "Duplicate") return; Status = "Ready"; ErrorMessage = null; ProcessedAt = null; }
}
