using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Patients.Commands.CompleteCare;

public sealed class CompleteCareCommandHandler : IRequestHandler<CompleteCareCommand>
{
    private static readonly string[] AdmissionRequirements =
    [
        "SOC_COMPLIANT", "DME_MED_SUPPLY", "NOA_FILED", "OASIS_SOC_COMPLETE",
        "QA_APPROVAL", "ORDERS_SIGNED", "DOCS_UPLOADED",
        "CASE_MIX_IDENTIFIED", "CASE_MIX_ORDERS_SIGNED", "CASE_MIX_PLOTTED"
    ];

    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IComplianceRepository _compliance;

    public CompleteCareCommandHandler(IPatientRepository patients, IUnitOfWork unitOfWork, IComplianceRepository compliance)
    { _patients = patients; _unitOfWork = unitOfWork; _compliance = compliance; }

    public async Task Handle(CompleteCareCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdForUpdateAsync(request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        var complianceRecords = await _compliance.GetByPatientAsync(request.PatientId, cancellationToken);
        var admissionComplete = AdmissionRequirements.All(code =>
            complianceRecords.Any(x => string.Equals(x.RequirementCode, code, StringComparison.OrdinalIgnoreCase) && x.IsCompleted));

        if (!admissionComplete)
            throw new InvalidOperationException("All admission compliance requirements must be completed before discharge.");

        var isTransfer = string.Equals(request.FinalStatus, "Transferred", StringComparison.OrdinalIgnoreCase);

        if (isTransfer && string.IsNullOrWhiteSpace(request.TransferDestination))
            throw new InvalidOperationException("Transfer destination is required when the final outcome is Transferred.");

        if (!isTransfer)
        {
            var dischargeSummarySigned = complianceRecords.Any(x =>
                string.Equals(x.RequirementCode, "DISCHARGE_SUMMARY_SIGNED", StringComparison.OrdinalIgnoreCase) && x.IsCompleted);

            if (!dischargeSummarySigned)
                throw new InvalidOperationException("Discharge Summary Signed must be completed before completing discharge.");
        }

        patient.UpdateWorkflowDetails(
            patient.PreAuthDueDate, patient.NumberOfVisits, patient.CaseMixType, patient.DmeMedSupplyNotes,
            patient.SocFeedbackFromPatient, patient.TifDate, patient.RocDate, patient.RecertDate, patient.PcpPtNotified,
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
