namespace CCAP.Domain.Entities;
public sealed class PatientCareLog
{
 private PatientCareLog(){}
 public Guid PatientCareLogId{get;private set;} public Guid PatientId{get;private set;} public Guid? RecordedByUserId{get;private set;} public string LogType{get;private set;}=""; public string Item{get;private set;}=""; public decimal? Quantity{get;private set;} public string? Unit{get;private set;} public DateTime RecordedAt{get;private set;} public string? Notes{get;private set;}
 public Patient Patient{get;private set;}=null!; public ApplicationUser? RecordedByUser{get;private set;}
 public PatientCareLog(Guid patientId,Guid? userId,string logType,string item,decimal? quantity,string? unit,string? notes){PatientCareLogId=Guid.NewGuid();PatientId=patientId;RecordedByUserId=userId;LogType=logType.Trim();Item=item.Trim();Quantity=quantity;Unit=unit?.Trim();Notes=notes?.Trim();RecordedAt=DateTime.UtcNow;}
}
