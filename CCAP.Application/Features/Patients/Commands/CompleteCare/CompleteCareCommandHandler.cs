using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Patients;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteCare;

public sealed class CompleteCareCommandHandler : IRequestHandler<CompleteCareCommand>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IComplianceRepository _compliance;

    public CompleteCareCommandHandler(
        IPatientRepository patients,
        IUnitOfWork unitOfWork,
        IComplianceRepository compliance)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
        _compliance = compliance;
    }

    public async Task Handle(CompleteCareCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdForUpdateAsync(request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        var complianceRecords = await _compliance.GetByPatientAsync(request.PatientId, cancellationToken);

        // Use the same canonical Yes/No/N/A compliance groups as the workflow progress UI.
        // Informational dates, numbers, notes, and dropdowns do not gate the final outcome.
        var admissionComplete = PatientWorkflowStageRules.IsGroupComplete(
            patient, complianceRecords, PatientWorkflowStageRules.Admission);
        var socComplete = PatientWorkflowStageRules.IsGroupComplete(
            patient, complianceRecords, PatientWorkflowStageRules.Soc);
        var ongoingCareComplete = PatientWorkflowStageRules.IsGroupComplete(
            patient, complianceRecords, PatientWorkflowStageRules.OngoingCare);
        var recertOrDischargeComplete = PatientWorkflowStageRules.IsGroupComplete(
            patient, complianceRecords, PatientWorkflowStageRules.RecertOrDischarge);
        if (!admissionComplete || !socComplete || !ongoingCareComplete ||
            !recertOrDischargeComplete)
        {
            throw new InvalidOperationException(
                "Complete the applicable Admission, SOC, On-Going Care, and Recert or Discharge compliance requirements before recording the final outcome.");
        }

        var isTransfer = string.Equals(request.FinalStatus, "Transferred", StringComparison.OrdinalIgnoreCase);

        if (isTransfer && string.IsNullOrWhiteSpace(request.TransferDestination))
            throw new InvalidOperationException("Transfer destination is required when the final outcome is Transferred.");

        // A transfer date is a valid alternative to a discharge date. Preserve
        // the existing discharge date for transfers and use the outcome date as
        // the transfer date instead.
        patient.UpdateWorkflowDetails(
            patient.PreAuthDueDate,
            patient.NumberOfVisits,
            patient.CaseMixType,
            patient.HospitalDcSummaryNotes,
            patient.DmeMedSupplyNotes,
            patient.SocFeedbackFromPatient,
            patient.TifDate,
            patient.RocDate,
            patient.RecertDate,
            patient.PcpPtNotified,
            !isTransfer ? request.OutcomeDate ?? patient.DischargeDate : patient.DischargeDate,
            !isTransfer ? request.DischargeFeedback ?? patient.DischargeFeedback : patient.DischargeFeedback,
            isTransfer ? request.TransferDestination ?? patient.TransferDestination : patient.TransferDestination,
            isTransfer ? request.OutcomeDate ?? patient.TransferDate : patient.TransferDate,
            isTransfer ? request.TransferReason ?? patient.TransferReason : patient.TransferReason);

        patient.CompleteCare(request.FinalStatus, request.FinalizedByUserId);
        _patients.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
