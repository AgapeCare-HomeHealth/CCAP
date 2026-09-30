using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;
namespace CCAP.Application.Features.Patients.Commands.UpdateWorkflowDetails;
public sealed class UpdateWorkflowDetailsCommandHandler : IRequestHandler<UpdateWorkflowDetailsCommand>
{
    private readonly IPatientRepository _patients; private readonly IUnitOfWork _unitOfWork; private readonly IPatientAuditLogRepository _auditLogs;
    public UpdateWorkflowDetailsCommandHandler(IPatientRepository patients,IUnitOfWork unitOfWork,IPatientAuditLogRepository auditLogs){_patients=patients;_unitOfWork=unitOfWork;_auditLogs=auditLogs;}
    public async Task Handle(UpdateWorkflowDetailsCommand r,CancellationToken ct){
        var p=await _patients.GetByIdForUpdateAsync(r.PatientId,ct)??throw new KeyNotFoundException("Patient not found.");
        p.UpdateWorkflowDetails(r.PreAuthDueDate,r.NumberOfVisits,r.CaseMixType,r.DmeMedSupplyNotes,r.SocFeedbackFromPatient,r.TifDate,r.RocDate,r.RecertDate,r.PcpPtNotified,r.DischargeDate,r.DischargeFeedback,r.TransferDestination,r.TransferDate,r.TransferReason);
        p.Activities.Add(new Activity(p.PatientId,r.UpdatedByUserId,"Workflow","Workflow details updated","Workflow dates and tracker information were updated."));
        await _auditLogs.AddAsync(new PatientAuditLog(p.PatientId,r.UpdatedByUserId,"PatientWorkflow",p.PatientId.ToString(),"UPDATE",null,"Workflow details updated","Workflow details updated."),ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
