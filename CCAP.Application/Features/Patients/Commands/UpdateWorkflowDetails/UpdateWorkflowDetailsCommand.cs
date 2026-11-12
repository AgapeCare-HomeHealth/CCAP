using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UpdateWorkflowDetails;
public sealed record UpdateWorkflowDetailsCommand(Guid PatientId, DateOnly? PreAuthDueDate, int? NumberOfVisits, string? CaseMixType, string? HospitalDcSummaryNotes, string? DmeMedSupplyNotes, string? SocFeedbackFromPatient, DateOnly? TifDate, DateOnly? RocDate, DateOnly? RecertDate, bool? PcpPtNotified, DateOnly? DischargeDate, string? DischargeFeedback, string? TransferDestination, DateOnly? TransferDate, string? TransferReason, string Section, Guid UpdatedByUserId, List<RecertificationRecordCommandDto>? RecertificationRecords = null) : IRequest;

public sealed class RecertificationRecordCommandDto
{
    public Guid Id { get; set; }
    public DateOnly? ReceivedDate { get; set; }
    public string? PcpPtNotified { get; set; }
}
