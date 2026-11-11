using CCAP.Domain.Entities;

namespace CCAP.Application.Features.Patients;

/// <summary>
/// Shared sequencing rules for the five patient workflow groups.
/// A requirement can only be completed after all preceding groups are complete.
/// </summary>
internal static class PatientWorkflowStageRules
{
    internal static readonly string[] Admission =
    [
        "PRE_AUTH_RECEIVED", "PRO_RECEIVED", "H_AND_P_RECEIVED", "F2F_RECEIVED", "PCP_CONFIRMED"
    ];

    internal static readonly string[] Soc =
    [
        "DME_MED_SUPPLY", "SIGNED_CONSENTS", "NOA_FILED",
        "OASIS_SOC_COMPLETE", "ORDERS_SIGNED", "DOCS_UPLOADED", "AVS_RECEIVED"
    ];

    // Only explicit Yes/No/N/A checklist items belong to workflow progression.
    internal static readonly string[] OngoingCare =
    [
        "CASE_MIX_IDENTIFIED", "CASE_MIX_PLOTTED", "QA_APPROVAL"
    ];

    internal static readonly string[] RecertOrDischarge =
    [
        "PCP_PT_NOTIFIED"
    ];

    internal static readonly string[] Discharge =
    [
        "DISCHARGE_SUMMARY_SIGNED", "NOMNC_SIGNED"
    ];

    internal static bool IsGroupComplete(Patient patient, IEnumerable<ComplianceRecord> records, IEnumerable<string> codes)
    {
        var items = records.ToList();
        return codes.All(code => items.Any(item => item.IsCompleted &&
            string.Equals(item.RequirementCode, code, StringComparison.OrdinalIgnoreCase)));
    }

    internal static string? GetPrerequisiteError(
        Patient patient,
        IEnumerable<ComplianceRecord> records,
        string requirementCode)
    {
        var items = records.ToList();
        var code = requirementCode.Trim().ToUpperInvariant();

        if (Admission.Contains(code, StringComparer.OrdinalIgnoreCase))
        {
            if (!patient.InsuranceVerifiedAt.HasValue)
                return "Complete Insurance Verification before completing Admission requirements.";
            return null;
        }

        if (!IsGroupComplete(patient, items, Admission))
            return "Complete all Admission requirements before completing items in the SOC group.";

        if (Soc.Contains(code, StringComparer.OrdinalIgnoreCase))
            return null;

        if (!IsGroupComplete(patient, items, Soc))
            return "Complete all SOC requirements before moving to On-Going Care.";

        if (OngoingCare.Contains(code, StringComparer.OrdinalIgnoreCase))
            return null;

        if (!IsGroupComplete(patient, items, OngoingCare))
            return "Complete the On-Going Care compliance requirements before moving to Recert or Discharge.";

        if (RecertOrDischarge.Contains(code, StringComparer.OrdinalIgnoreCase))
            return null;

        if (!IsGroupComplete(patient, items, RecertOrDischarge))
            return "Complete the Recert or Discharge requirements before completing Discharge items.";

        return null;
    }
}
