using System.Globalization;
using System.Text.RegularExpressions;
using CCAP.Application.Features.Referrals.DTOs;

namespace CCAP.Infrastructure.Referrals;

/// <summary>
/// Conservative, format-independent demographic reconciliation for scanned
/// referral packets. Operates on page-local OCR text to avoid mixing a patient's
/// details with physicians, pharmacy contacts and historical notes.
/// No patient-specific constants or source documents are stored in code.
/// </summary>
internal static class ScannedReferralDemographics
{
    private static readonly Regex PatientName = new(
        @"(?im)^\s*(?<last>[A-Z][A-Za-z'\-]{1,45}),\s*(?<first>[A-Z][A-Za-z'\-]{1,45})(?:\s+(?<middle>[A-Za-z][A-Za-z'\-]{0,45}))?(?=\s|$|\(|,)",
        RegexOptions.Compiled);

    private static readonly Regex BirthDate = new(
        @"(?i)\b(?:DOB|D\.?O\.?B\.?|Date\s+of\s+Birth|Birth\s*Date)\s*[:#-]?\s*(?<dob>\d{1,2}[/-]\d{1,2}[/-]\d{2,4})\b",
        RegexOptions.Compiled);

    private static readonly Regex PostalAddress = new(
        @"(?im)(?<street>\b\d{1,6}\s+[A-Z0-9][A-Za-z0-9.\-# /]+?(?:,\s*(?:APT|UNIT|SUITE|STE)\s*\S+)?),\s*(?<city>[A-Za-z][A-Za-z ]{1,45}),\s*(?<state>[A-Z]{2})\s*[,-]?\s*(?<zip>\d{5})(?:-\d{4})?\b",
        RegexOptions.Compiled);

    private static readonly Regex UsPhone = new(
        @"(?<!\d)(?:\+?1[-. ]?)?\(?\d{3}\)?[-. ]?\d{3}[-. ]?\d{4}(?!\d)",
        RegexOptions.Compiled);

    public static ReferralDocumentExtractionResult Apply(
        ReferralDocumentExtractionResult generic,
        IReadOnlyList<string> pageTexts,
        double confidence)
    {
        var names = new List<(string First, string Last, string? Middle, int Score)>();
        var dobVotes = new Dictionary<DateOnly, int>();
        var addresses = new List<(string Street, string City, string State, string Zip, int Score)>();
        var genders = new List<string>();
        var phones = new HashSet<string>(StringComparer.Ordinal);

        foreach (var page in pageTexts)
        {
            var isPatientPage = Regex.IsMatch(page,
                @"(?i)\b(?:ADMISSION RECORD|Patient Name|Resident Name|PROGRESS NOTES|HISTORY\s*&\s*PHYSICAL)\b");
            foreach (Match candidate in PatientName.Matches(page))
            {
                var first = candidate.Groups["first"].Value;
                var last = candidate.Groups["last"].Value;
                var middle = candidate.Groups["middle"].Success ? candidate.Groups["middle"].Value : null;
                if (first.Length <= 1 || last.Length <= 1) continue;
                if (middle is { Length: > 1 } && Regex.IsMatch(middle, @"(?i)^(?:DOB|MRN|Female|Male)$"))
                    middle = null;
                var score = (isPatientPage ? 2 : 0) + (middle is { Length: > 1 } ? 4 : 0);
                names.Add((first, last, middle, score));
            }

            foreach (Match match in BirthDate.Matches(page))
            {
                var parsed = ParseBirthDate(match.Groups["dob"].Value);
                if (parsed.HasValue)
                    dobVotes[parsed.Value] = dobVotes.GetValueOrDefault(parsed.Value) + 1;
            }

            foreach (Match match in PostalAddress.Matches(page))
            {
                var street = Clean(match.Groups["street"].Value);
                // Ignore provider addresses. The patient location appears near
                // resident information or explicitly-labelled home/address lines.
                var lineStart = page.LastIndexOf('\n', Math.Max(0, match.Index - 1)) + 1;
                var context = page[Math.Max(0, lineStart - 60)..match.Index];
                if (!isPatientPage && !Regex.IsMatch(context, @"(?i)\b(?:patient|resident|home|address)\b"))
                    continue;
                var score = (isPatientPage ? 3 : 0) +
                    (Regex.IsMatch(street, @"(?i)\b(?:APT|UNIT|SUITE|STE)\b") ? 3 : 0);
                addresses.Add((street, Clean(match.Groups["city"].Value),
                    match.Groups["state"].Value.ToUpperInvariant(), match.Groups["zip"].Value, score));
            }

            var genderMatch = Regex.Match(page, @"(?im)\bGender\s*[:#-]?\s*(Female|Male|F|M)\b");
            if (genderMatch.Success)
                genders.Add(NormalizeGender(genderMatch.Groups[1].Value));
            else if (Regex.IsMatch(page, @"(?i)\bLegal Sex\s+DOB\b"))
            {
                var legalRow = Regex.Match(page, @"(?im)^\s*[A-Za-z'\-]+,\s*[A-Za-z'\-]+(?:\s+[A-Za-z'\-]+)?\s+\d{4,}\s+(Female|Male)\s+\d{1,2}/\d{1,2}/\d{4}");
                if (legalRow.Success) genders.Add(NormalizeGender(legalRow.Groups[1].Value));
            }

            // Strictly patient-contact contexts; refuse to guess when several
            // different numbers appear across pages.
            var lines = page.Replace("\r", "").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i], @"(?i)\bPrevious Phone\s*#?\b") && i + 1 < lines.Length)
                    AddPhones(lines[i + 1]);
                else if (Regex.IsMatch(lines[i], @"(?i)^\s*(?:Home|Patient Phone|Primary Phone)\s*:")
                    && !Regex.IsMatch(lines[i], @"(?i)\b(?:pharmacy|provider|facility)\b"))
                    AddPhones(lines[i]);
            }
        }

        void AddPhones(string line)
        {
            foreach (Match phone in UsPhone.Matches(line))
            {
                var digits = Regex.Replace(phone.Value, @"\D", "");
                if (digits.Length == 11 && digits[0] == '1') digits = digits[1..];
                if (digits.Length == 10) phones.Add(digits);
            }
        }

        // Prefer the most complete repeated patient name. Do not infer middle
        // names from unrelated doctors or emergency contacts.
        var bestName = names.GroupBy(n => (n.First.ToUpperInvariant(), n.Last.ToUpperInvariant()))
            .OrderByDescending(group => group.Count())
            .ThenByDescending(group => group.Max(n => n.Score))
            .Select(group => group.OrderByDescending(n => n.Score).First())
            .FirstOrDefault();
        var bestDob = dobVotes.OrderByDescending(p => p.Value)
            .ThenBy(p => p.Key).Select(p => (DateOnly?)p.Key).FirstOrDefault();
        var bestAddress = addresses.OrderByDescending(a => a.Score).FirstOrDefault();
        var safePhone = phones.Count == 1 ? FormatUsPhone(phones.First()) : null;
        var gender = genders.GroupBy(g => g).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key;

        var validated = new ReferralDocumentExtractionResult
        {
            OcrUsed = true,
            Confidence = confidence,
            RawText = generic.RawText,
            DetectedForm = generic.DetectedForm,
            DetectedFormVersion = generic.DetectedFormVersion,
            MRN = generic.MRN,
            FirstName = bestName.First ?? generic.FirstName,
            LastName = bestName.Last ?? generic.LastName,
            MiddleName = bestName.Middle ?? generic.MiddleName,
            DateOfBirth = bestDob ?? generic.DateOfBirth,
            Gender = gender ?? generic.Gender,
            PrimaryPhone = safePhone, // never substitute a provider's phone
            AlternatePhone = null,
            StreetAddress = bestAddress.Street ?? generic.StreetAddress,
            City = bestAddress.City ?? generic.City,
            State = bestAddress.State ?? generic.State,
            ZipCode = bestAddress.Zip ?? generic.ZipCode,
            EmergencyContactName = null, // needs explicit relationship verification
            EmergencyContactRelationship = null,
            EmergencyContactPhone = null,
            ReferralNumber = generic.ReferralNumber,
            ReferralDate = generic.ReferralDate,
            ReferralSource = generic.ReferralSource,
            Priority = generic.Priority,
            PrimaryInsurance = generic.PrimaryInsurance,
            InsuranceMemberId = generic.InsuranceMemberId,
            AuthorizationDate = generic.AuthorizationDate,
            ApprovedVisits = generic.ApprovedVisits,
            ReferringPhysician = generic.ReferringPhysician,
            PhysicianPhone = generic.PhysicianPhone,
            PrimaryDiagnosis = generic.PrimaryDiagnosis,
            SecondaryDiagnosis = generic.SecondaryDiagnosis,
            OrderedServices = generic.OrderedServices,
            ReferralNotes = generic.ReferralNotes
        };
        validated.DetectedFieldCount = ReferralDocumentFieldParser.CountDetectedValues(validated);
        return validated;
    }

    private static DateOnly? ParseBirthDate(string candidate)
    {
        var parts = Regex.Split(candidate, "[-/]");
        if (parts.Length != 3) return null;
        if (!int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var day)
            || !int.TryParse(parts[2], out var year)) return null;
        if (parts[2].Length == 2) year += 1900;
        if (year < 1900 || year > DateTime.UtcNow.Year) return null;
        return DateOnly.TryParseExact($"{month:D2}/{day:D2}/{year:D4}", "MM/dd/yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;
    }

    private static string NormalizeGender(string value) =>
        value.Equals("F", StringComparison.OrdinalIgnoreCase) ? "Female" :
        value.Equals("M", StringComparison.OrdinalIgnoreCase) ? "Male" :
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());

    private static string FormatUsPhone(string digits) =>
        $"({digits[..3]}) {digits.Substring(3, 3)}-{digits[6..]}";

    private static string Clean(string value) => Regex.Replace(value.Trim(), @"\s{2,}", " ");
}
