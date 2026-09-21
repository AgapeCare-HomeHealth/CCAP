using MediatR;namespace CCAP.Application.Features.Patients.Commands.AddCareLog;
public sealed record AddCareLogCommand(Guid PatientId,string LogType,string Item,decimal? Quantity,string? Unit,string? Notes,Guid RecordedByUserId):IRequest<Guid>;
