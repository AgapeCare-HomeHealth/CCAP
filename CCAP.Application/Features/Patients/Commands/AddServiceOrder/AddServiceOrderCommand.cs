using MediatR;
namespace CCAP.Application.Features.Patients.Commands.AddServiceOrder;
public sealed record AddServiceOrderCommand(Guid PatientId, Guid ServiceTypeId, string? Frequency, string? Duration, bool IsPrimaryDiscipline, Guid RecordedByUserId) : IRequest<Guid>;
