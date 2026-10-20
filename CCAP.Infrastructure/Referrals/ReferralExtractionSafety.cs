using System.Text.RegularExpressions;
using CCAP.Application.Features.Referrals.DTOs;

namespace CCAP.Infrastructure.Referrals;

/// <summary>
/// Conservative checks on OCR/text-layer candidates. A matching label alone is not
/// proof that the following text is a patient identifier, physician, or payer.
/// Does not use patient-specific values, external services, or database lookups.
/// Insurance dropdown membership is verified separately by the Web application.
/// </summary>
internal static class ReferralExtractionSafety
{
    // Capture only the first identifier token after an explicit MRN label.
    // Deliberately do not accept generic "Patient ID"; it can be a different identifier.
    private static readonly Regex MrnLabel = new(
        @"(?i)(?<![A-Za-z])(?:M\.?R\.?N\.?|Medical\s+Record\s+(?:Number|No\.?|#))\s*(?:[:#=\-]\s*)?(?<id>[A-Z0-9][A-Z0-9\-]{2,19})(?![A-Z0-9\-])",
        RegexOptions.Compiled);

    private static readonly Regex ExplicitPhysician = new(
        @"(?i)^\s*(?:PCP|Primary\s+Care\s+(?:Physician|Provider)|Primary\s+Physician)\s*(?:Name\s*)?(?::|#|\-|\s{2,})\s*(?<name>[^\r\n]{3,100})$",
        RegexOptions.Compiled);

    private static readonly Regex PersonName = new(
        @"(?i)^(?:(?:Dr\.?|Doctor)\s+)?[A-Z][A-Za-z'.-]{1,30}(?:\s+[A-Z][A-Za-z'.-]{1,30}){1,3}(?:\s*,?\s*(?:MD|DO|NP|PA|FNP|DNP|PA-C))?\.?$",
        RegexOptions.Compiled);

    private static readonly Regex InvalidProviderTokens = new(
        @"(?i)\b(?:signature|signed|date\s+of\s+birth|patient|insurance|pharmacy|hospital|facility|health\s+center|medical\s+center|clinic|address|fax|telephone|phone|referral|admission|unknown)\b|\d",
        RegexOptions.Compiled);

    private static readonly Regex InvalidPayerTokens = new(
        @"(?i)\b(?:signature|gender|date\s+of\s+birth|DOB|MRN|medical\s+record|patient\s+name|member\s+(?:ID|number)|policy\s+number|authorization|approval|phone|fax|subscriber\s+ID)\b",
        RegexOptions.Compiled);

    public static ReferralDocumentExtractionResult Apply(
        ReferralDocumentExtractionResult result,
        IReadOnlyList<string> pageTexts)
    {
        var candidates = new Dictionary<string, ReferralMrnCandidate>(StringComparer.OrdinalIgnoreCase);
        var physicianCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < pageTexts.Count; index++)
        {
            var text = pageTexts[index];
            foreach (Match match in MrnLabel.Matches(text))
            {
                var mrn = match.Groups["id"].Value.TrimEnd('-', '.').ToUpperInvariant();
                if (!IsValidMrn(mrn)) continue;

                // Retain the first page as source evidence for repeated MRNs.
                candidates.TryAdd(mrn, new ReferralMrnCandidate
                {
                    Value = mrn,
                    PageNumber = index + 1,
                    Source = DetectPageSection(text)
                });
            }

            foreach (var line in text.Replace("\r", "").Split('\n'))
            {
                var match = ExplicitPhysician.Match(line.Trim());
                if (!match.Success) continue;
                var name = CleanPhysician(match.Groups["name"].Value);
                if (name is not null) physicianCandidates.Add(name);
            }
        }

        result.MrnCandidates = candidates.Values.OrderBy(x => x.PageNumber).ToList();
        result.MrnRequiresReview = result.MrnCandidates.Count > 1;
        result.MRN = result.MrnCandidates.Count == 1 ? result.MrnCandidates[0].Value : null;

        // A physician must be associated with an explicit primary-care/PCP label.
        // Do not preserve the generic field reader's "Physician"/"Signature" result.
        result.ReferringPhysician = physicianCandidates.Count == 1
            ? physicianCandidates.First()
            : null;

        // Reject malformed OCR values. The actual payer must also be an exact
        // member of the active Insurance dropdown (checked in UploadStep).
        result.PrimaryInsurance = CleanInsurance(result.PrimaryInsurance);
        result.DetectedFieldCount = ReferralDocumentFieldParser.CountDetectedValues(result);
        return result;
    }

    private static bool IsValidMrn(string value)
    {
        if (value.Length is < 4 or > 20) return false;
        var digitCount = value.Count(char.IsDigit);
        return digitCount >= 3 && Regex.IsMatch(value, @"^[A-Z0-9]+(?:-[A-Z0-9]+)*$");
    }

    private static string DetectPageSection(string text)
    {
        var top = text.Length > 1800 ? text[..1800] : text;
        if (Regex.IsMatch(top, @"(?i)\bADMISSION RECORD\b")) return "Admission record";
        if (Regex.IsMatch(top, @"(?i)\bHISTORY\s*&\s*PHYSICAL\b")) return "Hospital history / physical";
        if (Regex.IsMatch(top, @"(?i)\bPROGRESS NOTES\b")) return "Progress notes";
        return "Referral document";
    }

    private static string? CleanPhysician(string raw)
    {
        // A second field on the same physical row invalidates the candidate.
        var candidate = Regex.Replace(raw.Trim(), @"\s{2,}.*$", "").Trim();
        candidate = candidate.Trim(' ', ',', ';', ':');
        if (candidate.Length is < 5 or > 85 || InvalidProviderTokens.IsMatch(candidate)) return null;
        return PersonName.IsMatch(candidate) ? candidate : null;
    }

    private static string? CleanInsurance(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var clean = Regex.Replace(value.Trim(), @"\s+", " ");
        if (clean.Length is < 4 or > 90) return null;
        if (clean.Count(char.IsLetter) < 4) return null;
        if (InvalidPayerTokens.IsMatch(clean)) return null;
        if (clean.Contains(':') || clean.Contains(';') || clean.Contains('|')) return null;
        return clean;
    }
}
