namespace CCAP.Web.Features.Tracker.ReferralTracker.Models;

/// <summary>
/// Centralized display mapping for Referral Tracker status colors.
/// Status values remain business data; this class only determines presentation.
/// </summary>
public static class ReferralTrackerStatusStyle
{
    private static readonly Dictionary<string, (string Label, string Background, string Foreground)> Styles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["DISCHARGE FROM HOSPITAL/SNF"] = ("Discharge from Hospital/SNF", "#F4B183", "#3B2408"),
            ["SOC"] = ("SOC", "#006100", "#FFFFFF"),
            ["READY FOR SOC"] = ("Ready for SOC", "#A9D18E", "#17320B"),
            ["NON-ADMIT/DECLINED"] = ("Non-Admit/Declined", "#C00000", "#FFFFFF"),
            ["NON-ADMIT/INSURANCE"] = ("Non-Admit/Insurance", "#F4CCCC", "#641010"),
            ["NON-ADMIT/CLINICAL"] = ("Non-Admit/Clinical", "#F4CCCC", "#641010"),
            ["WENT TO DIFFERENT HH"] = ("Went to Different HH", "#C00000", "#FFFFFF"),
            ["DEAD REFERRAL"] = ("Dead Referral", "#4472C4", "#FFFFFF"),
            ["EXPIRED AND NEED NEW REF"] = ("Expired and Need New Ref", "#806000", "#FFFFFF"),
            ["STILL AT THE HOSPITAL"] = ("Still at the Hospital", "#D9D9D9", "#202020"),
            ["PRO BONO"] = ("Pro Bono", "#D9B3E6", "#382044"),
            ["WAITING FOR PRE AUTH"] = ("Waiting for Pre Auth", "#FFFF00", "#262000"),
            ["NO STATUS OR NEW REF"] = ("No Status or New Ref", "#A6A6A6", "#202020")
        };

    public static (string Label, string Background, string Foreground) Resolve(string? status)
    {
        if (!string.IsNullOrWhiteSpace(status) && Styles.TryGetValue(status.Trim(), out var style))
            return style;

        return Styles["NO STATUS OR NEW REF"];
    }

    public static IReadOnlyList<(string Label, string Background, string Foreground)> All =>
        Styles.Values.ToList();
}
