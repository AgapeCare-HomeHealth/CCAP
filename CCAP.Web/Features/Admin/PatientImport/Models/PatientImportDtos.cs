namespace CCAP.Web.Features.Admin.PatientImport.Models;

public sealed class PatientImportBatchDto
{
    public Guid PatientImportBatchId { get; set; }
    public string FileName { get; set; } = "";
    public string UploadedBy { get; set; } = "";
    public string Status { get; set; } = "";
    public int TotalRows { get; set; }
    public int InsertedRows { get; set; }
    public int DuplicateRows { get; set; }
    public int FailedRows { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Summary { get; set; }
    public List<PatientImportRowDto> Rows { get; set; } = [];
}
public sealed class PatientImportRowDto
{
    public Guid PatientImportRowId { get; set; }
    public int RowNumber { get; set; }
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
}
