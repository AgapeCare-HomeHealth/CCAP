namespace CCAP.Application.Features.Patients;

/// <summary>Only explicit Yes/No/N/A compliance checklist items create actionable tasks.</summary>
public static class PatientComplianceTaskCatalog
{
    public static readonly IReadOnlyDictionary<string, (string Title, string Description)> Items =
        new Dictionary<string, (string Title, string Description)>(StringComparer.OrdinalIgnoreCase)
    {
        ["PRE_AUTH_RECEIVED"] = ("Pre-Auth Received", "Confirm pre-authorization has been received."),
        ["PRO_RECEIVED"] = ("PRO Received", "Confirm PRO has been received."),
        ["H_AND_P_RECEIVED"] = ("H&P", "Confirm H&P has been received."),
        ["F2F_RECEIVED"] = ("F2F Received", "Confirm Face-to-Face documentation has been received."),
        ["PCP_CONFIRMED"] = ("PCP Confirmed", "Confirm the primary care physician."),
        ["DME_MED_SUPPLY"] = ("DME / Med Supply", "Confirm DME and medical supplies."),
        ["SIGNED_CONSENTS"] = ("Signed Consents", "Confirm required consents are signed."),
        ["NOA_FILED"] = ("NOA", "Confirm the Notice of Admission is filed."),
        ["OASIS_SOC_COMPLETE"] = ("OASIS / SOC Compliant", "Confirm OASIS / SOC compliance."),
        ["ORDERS_SIGNED"] = ("Orders Signed by Physician", "Confirm orders are signed by the physician."),
        ["DOCS_UPLOADED"] = ("Docs Uploaded", "Confirm required documents are uploaded."),
        ["AVS_RECEIVED"] = ("AVS Received", "Confirm AVS has been received."),
        ["CASE_MIX_IDENTIFIED"] = ("Case Mix Identified", "Confirm case mix has been identified."),
        ["CASE_MIX_PLOTTED"] = ("Case Mix Plotted", "Confirm case mix has been plotted."),
        ["QA_APPROVAL"] = ("QA Approval — PO/POC Ready for Faxing", "Confirm QA approval and PO/POC readiness for faxing."),
        ["DISCHARGE_SUMMARY_SIGNED"] = ("DC Summary Signed", "Confirm the discharge summary is signed."),
        ["NOMNC_SIGNED"] = ("NOMNC Signed", "Confirm NOMNC is signed.")
    };

    public static string TaskTitle(string requirementCode) =>
        Items.TryGetValue(requirementCode, out var item) ? $"Compliance: {item.Title}" : $"Compliance: {requirementCode}";
}
