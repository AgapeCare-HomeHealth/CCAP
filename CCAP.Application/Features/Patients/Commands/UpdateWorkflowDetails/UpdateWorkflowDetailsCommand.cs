using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UpdateWorkflowDetails;
public sealed record UpdateWorkflowDetailsCommand(Guid PatientId, DateOnly? PreAuthDueDate, int? NumberOfVisits, string? CaseMixType, string? DmeMedSupplyNotes, string? SocFeedbackFromPatient, DateOnly? TifDate, DateOnly? RocDate, DateOnly? RecertDate, bool? PcpPtNotified, DateOnly? DischargeDate, string? DischargeFeedback, string? TransferDestination, DateOnly? TransferDate, string? TransferReason, Guid UpdatedByUserId) : IRequest;
