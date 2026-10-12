using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CCAP.Application.Abstractions.Referrals;
using CCAP.Application.Features.Referrals.DTOs;
using Microsoft.Extensions.Configuration;
using TesseractOCR;
using TesseractOCR.Enums;
using UglyToad.PdfPig;

namespace CCAP.Infrastructure.Referrals;

/// <summary>
/// Extracts referral data from multiple clinic/form versions.
/// Form profiles are configuration-driven: a clinic can add a new version by
/// defining detection markers and label aliases without changing this service.
/// </summary>
public sealed class ReferralDocumentExtractionService : IReferralDocumentExtractionService
{
    private readonly IConfiguration _configuration;

    public ReferralDocumentExtractionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ReferralDocumentExtractionResult> ExtractAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default)
    {
        if (pdfStream is null)
            throw new ArgumentNullException(nameof(pdfStream));

        await using var buffer = new MemoryStream();
        await pdfStream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        if (bytes.Length == 0)
            throw new ArgumentException("The referral PDF is empty.", nameof(pdfStream));

        var digitalText = new StringBuilder();
        var imagePayloads = new List<byte[]>();

        using (var document = PdfDocument.Open(bytes))
        {
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Build text from word coordinates instead of relying on the
                // PDF content-stream order. Clinic referral forms are usually
                // tables/columns, so preserving the horizontal positions is
                // critical for mapping a value to the correct label.
                var layoutText = BuildPositionedPageText(page);
                if (!string.IsNullOrWhiteSpace(layoutText))
                    digitalText.AppendLine(layoutText);
                else if (!string.IsNullOrWhiteSpace(page.Text))
                    digitalText.AppendLine(page.Text);

                foreach (var image in page.GetImages())
                {
                    if (image.TryGetPng(out var png) && png is { Length: > 0 })
                        imagePayloads.Add(png);
                    else
                    {
                        var raw = image.RawBytes.ToArray();
                        if (raw.Length > 0)
                            imagePayloads.Add(raw);
                    }
                }
            }
        }

        var textValue = digitalText.ToString();
        var ocrUsed = false;
        double? confidence = null;
        var profiles = ReferralFormProfileLoader.Load(_configuration);

        // First pass: use coordinate-aware PDF text. Do not trim leading
        // whitespace here; the parser uses those positions for table columns.
        var result = ReferralDocumentFieldParser.Parse(textValue, false, null, profiles);

        // Second pass: if the PDF contains images and the text pass did not
        // recover useful referral values, run OCR as a fallback. Do not require
        // the form to match a registered clinic profile before attempting OCR.
        // This is important because scanned forms and new clinic versions are
        // intentionally allowed to return a partially populated result.
        if (imagePayloads.Count > 0 && ReferralDocumentFieldParser.CountDetectedValues(result) < 2)
        {
            try
            {
                var ocr = await RunOcrAsync(imagePayloads, cancellationToken);
                if (!string.IsNullOrWhiteSpace(ocr.Text))
                {
                    var ocrResult = ReferralDocumentFieldParser.Parse(ocr.Text, true, ocr.Confidence, profiles);
                    result = ReferralDocumentFieldParser.MergeMissing(result, ocrResult);
                    ocrUsed = true;
                    confidence = ocr.Confidence;
                }
            }
            catch (InvalidOperationException)
            {
                // OCR is best-effort. If Tesseract is not configured, keep the
                // digital-text result and allow manual entry instead of making
                // PDF extraction a blocking requirement.
            }
        }

        return result;
    }

    private static string BuildPositionedPageText(UglyToad.PdfPig.Content.Page page)
    {
        var words = page.GetWords()
            .Where(w => !string.IsNullOrWhiteSpace(w.Text))
            .OrderByDescending(w => w.BoundingBox.Top)
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        if (words.Count == 0)
            return string.Empty;

        var rows = new List<List<UglyToad.PdfPig.Content.Word>>();
        const double yTolerance = 3.0;

        foreach (var word in words)
        {
            var row = rows.FirstOrDefault(r =>
            {
                var referenceY = r[0].BoundingBox.Bottom;
                return Math.Abs(referenceY - word.BoundingBox.Bottom) <= yTolerance;
            });

            if (row is null)
            {
                row = [];
                rows.Add(row);
            }

            row.Add(word);
        }

        var output = new StringBuilder();
        const double pointsPerColumn = 3.2;

        foreach (var row in rows.OrderByDescending(r => r.Max(w => w.BoundingBox.Top)))
        {
            var line = new StringBuilder();
            var currentColumn = 0;

            foreach (var word in row.OrderBy(w => w.BoundingBox.Left))
            {
                var targetColumn = Math.Max(0, (int)Math.Round(word.BoundingBox.Left / pointsPerColumn));
                if (targetColumn > currentColumn)
                    line.Append(' ', targetColumn - currentColumn);
                else if (line.Length > 0)
                    line.Append(' ');

                line.Append(word.Text);
                currentColumn = line.Length;
            }

            if (line.Length > 0)
                output.AppendLine(line.ToString().TrimEnd());
        }

        return output.ToString();
    }

    private async Task<(string Text, double Confidence)> RunOcrAsync(
        IReadOnlyCollection<byte[]> images,
        CancellationToken cancellationToken)
    {
        var tessdataPath = _configuration["ReferralDocumentOcr:TessdataPath"];
        if (string.IsNullOrWhiteSpace(tessdataPath))
            tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");

        if (!File.Exists(Path.Combine(tessdataPath, "eng.traineddata")))
        {
            throw new InvalidOperationException(
                "The uploaded referral appears to be a scanned PDF, but OCR is not configured. " +
                "Place eng.traineddata in the configured ReferralDocumentOcr:TessdataPath folder.");
        }

        var text = new StringBuilder();
        var confidenceTotal = 0d;
        var confidenceCount = 0;

        using var engine = new Engine(tessdataPath, Language.English, EngineMode.Default);

        foreach (var imageBytes in images.Take(20))
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var image = TesseractOCR.Pix.Image.LoadFromMemory(imageBytes);
            using var page = engine.Process(image);

            text.AppendLine(page.Text);
            confidenceTotal += page.MeanConfidence;
            confidenceCount++;
            await Task.Yield();
        }

        return (
            text.ToString(),
            confidenceCount == 0 ? 0 : confidenceTotal / confidenceCount);
    }
}

internal sealed class ReferralFormProfile
{
    public string Name { get; init; } = "Generic";
    public string Version { get; init; } = "1";
    public int Priority { get; init; }
    public List<string> DetectionMarkers { get; init; } = [];
    public Dictionary<string, List<string>> FieldAliases { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ServiceLabels { get; init; } = [];
}

internal static class ReferralFormProfileLoader
{
    public static IReadOnlyList<ReferralFormProfile> Load(IConfiguration configuration)
    {
        var section = configuration.GetSection("ReferralDocumentForms");
        var profiles = new List<ReferralFormProfile>();

        foreach (var child in section.GetChildren())
        {
            var profile = new ReferralFormProfile
            {
                Name = child["Name"] ?? child.Key,
                Version = child["Version"] ?? "1",
                Priority = int.TryParse(child["Priority"], out var priority) ? priority : 0,
                DetectionMarkers = child.GetSection("DetectionMarkers").GetChildren()
                    .Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToList(),
                ServiceLabels = child.GetSection("ServiceLabels").GetChildren()
                    .Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToList()
            };

            foreach (var field in child.GetSection("FieldAliases").GetChildren())
            {
                var aliases = field.GetChildren().Select(x => x.Value)
                    .Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToList();
                if (aliases.Count > 0)
                    profile.FieldAliases[field.Key] = aliases;
            }

            profiles.Add(profile);
        }

        return profiles.Count == 0
            ? [ReferralFormProfileDefaults.CreateGeneric()]
            : profiles.OrderByDescending(x => x.Priority).ToList();
    }
}

internal static class ReferralFormProfileDefaults
{
    public static ReferralFormProfile CreateGeneric() => new()
    {
        Name = "Generic Home Health Referral",
        Version = "1",
        DetectionMarkers = ["referral", "patient", "home health"],
        FieldAliases = CreateAliases()
    };

    public static Dictionary<string, List<string>> CreateAliases() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["MRN"] = ["MRN", "Medical Record Number", "Medical Record #", "Patient ID"],
            ["PatientName"] = ["Patient Name", "Patient Full Name", "Client Name", "Name of Patient", "Member Name"],
            ["FirstName"] = ["First Name", "Given Name", "Forename"],
            ["MiddleName"] = ["Middle Name", "Middle Initial"],
            ["LastName"] = ["Last Name", "Surname", "Family Name"],
            ["DateOfBirth"] = ["Date of Birth", "DOB", "Birth Date", "Birthdate", "Date Born"],
            ["Gender"] = ["Gender", "Sex"],
            ["PrimaryPhone"] = ["Primary Phone", "Phone", "Telephone", "Mobile", "Home Phone"],
            ["AlternatePhone"] = ["Alternate Phone", "Secondary Phone", "Other Phone"],
            ["StreetAddress"] = ["Street Address", "Address", "Patient Address", "Home Address"],
            ["City"] = ["City", "Town"],
            ["State"] = ["State", "Province", "State/Province"],
            ["ZipCode"] = ["ZIP", "Zip Code", "Postal Code", "ZIP Code"],
            ["EmergencyContactName"] = ["Emergency Contact", "Emergency Contact Name", "Next of Kin"],
            ["EmergencyContactRelationship"] = ["Emergency Contact Relationship", "Relationship", "Relation"],
            ["EmergencyContactPhone"] = ["Emergency Contact Phone", "Emergency Phone", "Next of Kin Phone"],
            ["ReferralNumber"] = ["Referral Number", "Referral #", "Referral ID", "Referral No", "Referral Number/ID", "Referral ID/No"],
            ["ReferralDate"] = ["Referral Date", "Date Referred", "Date of Referral"],
            ["ReferralSource"] = ["Referral Source", "Referred By", "Referral From", "Referring Facility", "Facility"],
            ["Priority"] = ["Priority", "Referral Priority", "Urgency"],
            ["PrimaryInsurance"] = ["Primary Insurance", "Insurance", "Payer", "Primary Payer", "Primary Payor"],
            ["InsuranceMemberId"] = ["Member ID", "Member Number", "Policy Number", "Insurance ID", "Member / Policy ID"],
            ["AuthorizationDate"] = ["Authorization Date", "Auth Date"],
            ["ApprovedVisits"] = ["Approved Visits", "Visits Approved", "Authorized Visits"],
            ["ReferringPhysician"] = ["Referring Physician", "Referring Provider", "Physician", "Doctor", "Ordering Provider", "Ordering MD", "Attending Physician"],
            ["PhysicianPhone"] = ["Physician Phone", "Provider Phone", "Doctor Phone"],
            ["PrimaryDiagnosis"] = ["Primary Diagnosis", "Diagnosis", "Dx", "Primary Dx"],
            ["SecondaryDiagnosis"] = ["Secondary Diagnosis", "Secondary Dx"],
            ["ReferralNotes"] = ["Referral Notes", "Clinical Notes", "Notes", "Reason for Referral", "Clinical Summary"]
        };
}

internal static class ReferralDocumentFieldParser
{
    public static ReferralDocumentExtractionResult Parse(
        string rawText,
        bool ocrUsed,
        double? confidence,
        IReadOnlyList<ReferralFormProfile> profiles)
    {
        var lines = rawText.Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.TrimEnd())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var profile = DetectProfile(rawText, profiles);
        var aliases = MergeAliases(profile.FieldAliases, ReferralFormProfileDefaults.CreateAliases());
        var parser = new ReferralFieldReader(lines, aliases);

        var fullName = parser.Find("PatientName");
        var firstName = parser.Find("FirstName") ?? GetNamePart(fullName, 0) ?? parser.FindFullNamePart(0);
        var middleName = parser.Find("MiddleName") ?? GetNamePart(fullName, 1) ?? parser.FindFullNamePart(1);
        var lastName = parser.Find("LastName") ?? GetLastName(fullName) ?? parser.FindFullNamePart(-1);
        var dateOfBirth = ParseDate(parser.FindDate("DateOfBirth"));
        var streetAddress = parser.FindMultiline("StreetAddress", "City", "State", "ZipCode");
        var city = parser.Find("City");
        var state = parser.Find("State");
        var zipCode = parser.Find("ZipCode");
        InferAddressParts(streetAddress, ref city, ref state, ref zipCode);

        var primaryPhone = NormalizePhone(parser.Find("PrimaryPhone"));
        var alternatePhone = NormalizePhone(parser.Find("AlternatePhone"));
        var emergencyPhone = NormalizePhone(parser.Find("EmergencyContactPhone"));
        var gender = NormalizeGender(parser.Find("Gender"));
        var referralDate = ParseDateTime(parser.Find("ReferralDate"));
        var authorizationDate = ParseDate(parser.Find("AuthorizationDate"));
        var approvedVisits = ParseInt(parser.Find("ApprovedVisits"));
        var orderedServices = parser.FindServices(profile.ServiceLabels);

        var result = new ReferralDocumentExtractionResult
        {
            OcrUsed = ocrUsed,
            Confidence = confidence,
            RawText = rawText,
            DetectedForm = profile.Name,
            DetectedFormVersion = profile.Version,
            MRN = CleanOptional(parser.Find("MRN")),
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            PrimaryPhone = primaryPhone,
            AlternatePhone = alternatePhone,
            StreetAddress = streetAddress,
            City = city,
            State = state,
            ZipCode = zipCode,
            EmergencyContactName = parser.Find("EmergencyContactName"),
            EmergencyContactRelationship = parser.Find("EmergencyContactRelationship"),
            EmergencyContactPhone = emergencyPhone,
            ReferralNumber = parser.Find("ReferralNumber"),
            ReferralDate = referralDate,
            ReferralSource = parser.Find("ReferralSource"),
            Priority = parser.Find("Priority"),
            PrimaryInsurance = parser.Find("PrimaryInsurance"),
            InsuranceMemberId = parser.Find("InsuranceMemberId"),
            AuthorizationDate = authorizationDate,
            ApprovedVisits = approvedVisits,
            ReferringPhysician = parser.Find("ReferringPhysician"),
            PhysicianPhone = NormalizePhone(parser.Find("PhysicianPhone")),
            PrimaryDiagnosis = parser.Find("PrimaryDiagnosis"),
            SecondaryDiagnosis = parser.Find("SecondaryDiagnosis"),
            OrderedServices = orderedServices,
            ReferralNotes = parser.Find("ReferralNotes") ?? parser.FindSectionNotes()
        };

        result.DetectedFieldCount = CountDetectedValues(result);
        return result;
    }

    private static string? GetNamePart(string? fullName, int index)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return null;
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return index == 0 ? parts.FirstOrDefault() : null;
        if (index == 0) return parts[0];
        if (index == 1 && parts.Length >= 3) return parts[1];
        return null;
    }

    private static string? GetLastName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return null;
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= 2 ? parts[^1] : null;
    }

    private static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = Regex.Replace(value, @"\D", "");
        return digits.Length >= 7 && digits.Length <= 15 ? value.Trim() : null;
    }

    private static string? NormalizeGender(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "male" or "m" => "Male",
            "female" or "f" => "Female",
            "other" => "Other",
            "unknown" => "Unknown",
            _ => null
        };
    }

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void InferAddressParts(
        string? address,
        ref string? city,
        ref string? state,
        ref string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(address)) return;

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            var zipMatch = Regex.Match(address, @"(?<!\d)(\d{4,6})(?!\d)\s*$");
            if (zipMatch.Success) zipCode = zipMatch.Groups[1].Value;
        }

        var withoutZip = Regex.Replace(address, @"\s+\d{4,6}\s*$", "").Trim();
        var segments = withoutZip.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length >= 3)
        {
            if (string.IsNullOrWhiteSpace(state)) state = segments[^1];
            if (string.IsNullOrWhiteSpace(city)) city = segments[^2];
        }
    }

    public static int CountDetectedValues(ReferralDocumentExtractionResult result)
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(result.MRN)) count++;
        if (!string.IsNullOrWhiteSpace(result.FirstName)) count++;
        if (!string.IsNullOrWhiteSpace(result.LastName)) count++;
        if (result.DateOfBirth.HasValue) count++;
        if (!string.IsNullOrWhiteSpace(result.ReferralNumber)) count++;
        if (result.ReferralDate.HasValue) count++;
        if (!string.IsNullOrWhiteSpace(result.ReferralSource)) count++;
        if (!string.IsNullOrWhiteSpace(result.ReferringPhysician)) count++;
        if (!string.IsNullOrWhiteSpace(result.PrimaryInsurance)) count++;
        if (!string.IsNullOrWhiteSpace(result.PrimaryDiagnosis)) count++;
        if (result.OrderedServices.Count > 0) count++;
        return count;
    }

    public static ReferralDocumentExtractionResult MergeMissing(
        ReferralDocumentExtractionResult primary,
        ReferralDocumentExtractionResult fallback)
    {
        var merged = new ReferralDocumentExtractionResult
        {
            OcrUsed = primary.OcrUsed || fallback.OcrUsed,
            Confidence = primary.Confidence ?? fallback.Confidence,
            RawText = string.Join(Environment.NewLine, new[] { primary.RawText, fallback.RawText }.Where(x => !string.IsNullOrWhiteSpace(x))),
            DetectedForm = primary.DetectedForm ?? fallback.DetectedForm,
            DetectedFormVersion = primary.DetectedFormVersion ?? fallback.DetectedFormVersion,
            MRN = First(primary.MRN, fallback.MRN),
            FirstName = First(primary.FirstName, fallback.FirstName),
            MiddleName = First(primary.MiddleName, fallback.MiddleName),
            LastName = First(primary.LastName, fallback.LastName),
            DateOfBirth = primary.DateOfBirth ?? fallback.DateOfBirth,
            Gender = First(primary.Gender, fallback.Gender),
            PrimaryPhone = First(primary.PrimaryPhone, fallback.PrimaryPhone),
            AlternatePhone = First(primary.AlternatePhone, fallback.AlternatePhone),
            StreetAddress = First(primary.StreetAddress, fallback.StreetAddress),
            City = First(primary.City, fallback.City),
            State = First(primary.State, fallback.State),
            ZipCode = First(primary.ZipCode, fallback.ZipCode),
            EmergencyContactName = First(primary.EmergencyContactName, fallback.EmergencyContactName),
            EmergencyContactRelationship = First(primary.EmergencyContactRelationship, fallback.EmergencyContactRelationship),
            EmergencyContactPhone = First(primary.EmergencyContactPhone, fallback.EmergencyContactPhone),
            ReferralNumber = First(primary.ReferralNumber, fallback.ReferralNumber),
            ReferralDate = primary.ReferralDate ?? fallback.ReferralDate,
            ReferralSource = First(primary.ReferralSource, fallback.ReferralSource),
            Priority = First(primary.Priority, fallback.Priority),
            PrimaryInsurance = First(primary.PrimaryInsurance, fallback.PrimaryInsurance),
            InsuranceMemberId = First(primary.InsuranceMemberId, fallback.InsuranceMemberId),
            AuthorizationDate = primary.AuthorizationDate ?? fallback.AuthorizationDate,
            ApprovedVisits = primary.ApprovedVisits ?? fallback.ApprovedVisits,
            ReferringPhysician = First(primary.ReferringPhysician, fallback.ReferringPhysician),
            PhysicianPhone = First(primary.PhysicianPhone, fallback.PhysicianPhone),
            PrimaryDiagnosis = First(primary.PrimaryDiagnosis, fallback.PrimaryDiagnosis),
            SecondaryDiagnosis = First(primary.SecondaryDiagnosis, fallback.SecondaryDiagnosis),
            OrderedServices = primary.OrderedServices.Count > 0 ? primary.OrderedServices : fallback.OrderedServices,
            ReferralNotes = First(primary.ReferralNotes, fallback.ReferralNotes)
        };

        merged.DetectedFieldCount = CountDetectedValues(merged);
        return merged;
    }

    private static string? First(string? primary, string? fallback) =>
        string.IsNullOrWhiteSpace(primary) ? fallback : primary;

    private static ReferralFormProfile DetectProfile(string rawText, IReadOnlyList<ReferralFormProfile> profiles)
    {
        var normalized = Normalize(rawText);
        var best = profiles.FirstOrDefault() ?? ReferralFormProfileDefaults.CreateGeneric();
        var bestScore = -1;

        foreach (var profile in profiles)
        {
            var score = profile.DetectionMarkers.Count(marker => normalized.Contains(Normalize(marker), StringComparison.OrdinalIgnoreCase));
            if (score > bestScore)
            {
                best = profile;
                bestScore = score;
            }
        }

        return best;
    }

    private static Dictionary<string, List<string>> MergeAliases(
        Dictionary<string, List<string>> configured,
        Dictionary<string, List<string>> defaults)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in defaults)
            result[pair.Key] = pair.Value.ToList();

        foreach (var pair in configured)
        {
            result[pair.Key] = pair.Value
                .Concat(result.TryGetValue(pair.Key, out var existing) ? existing : [])
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        return result;
    }

    private static string Normalize(string value) =>
        Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9]+", " ").Trim();

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var formats = new[] { "MM/dd/yyyy", "M/d/yyyy", "MM-dd-yyyy", "M-d-yyyy", "yyyy-MM-dd", "yyyy/MM/dd", "MMM d, yyyy", "MMMM d, yyyy" };
        return DateOnly.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date)
            ? date
            : DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date) ? date : null;
    }

    private static DateTime? ParseDateTime(string? value) => ParseDate(value)?.ToDateTime(TimeOnly.MinValue);

    private static int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var match = Regex.Match(value, @"\d+");
        return match.Success && int.TryParse(match.Value, out var result) ? result : null;
    }

    private sealed class ReferralFieldReader
    {
        private readonly string[] _lines;
        private readonly Dictionary<string, List<string>> _aliases;
        private readonly HashSet<string> _allLabels;

        public ReferralFieldReader(string[] lines, Dictionary<string, List<string>> aliases)
        {
            _lines = lines;
            _aliases = aliases;
            _allLabels = aliases.Values.SelectMany(x => x).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public string? Find(string field)
        {
            if (!_aliases.TryGetValue(field, out var labels)) return null;

            // Same-line form: Label: Value
            foreach (var line in _lines)
            {
                foreach (var label in labels.OrderByDescending(x => x.Length))
                {
                    var match = Regex.Match(line, $@"^\s*{Regex.Escape(label)}\s*[:#-]\s*(.+?)\s*$", RegexOptions.IgnoreCase);
                    if (match.Success) return Clean(match.Groups[1].Value);
                }
            }

            // Column/table layout: several labels can share one row and their
            // corresponding values are on the next row. Use the character
            // positions preserved by digital PDF extraction to isolate the
            // value under the requested label.
            var columnValue = FindColumnValue(labels);
            if (!string.IsNullOrWhiteSpace(columnValue))
                return columnValue;

            // Label on one line, value on the next line(s). This is the common
            // layout used by clinic forms and survives OCR line breaks.
            for (var i = 0; i < _lines.Length; i++)
            {
                if (!IsLabelLine(_lines[i], labels)) continue;

                for (var j = i + 1; j < _lines.Length && j <= i + 4; j++)
                {
                    if (IsAnyLabelLine(_lines[j])) break;
                    var value = Clean(_lines[j]);
                    if (!string.IsNullOrWhiteSpace(value) && !IsSectionHeading(value))
                        return value;
                }
            }

            // Inline label without punctuation, e.g. "DOB 06/14/1948".
            foreach (var line in _lines)
            {
                foreach (var label in labels.OrderByDescending(x => x.Length))
                {
                    var match = Regex.Match(line, $@"^\s*{Regex.Escape(label)}\s+(.+?)\s*$", RegexOptions.IgnoreCase);
                    if (match.Success && !IsSectionHeading(match.Groups[1].Value)) return Clean(match.Groups[1].Value);
                }
            }

            return null;
        }

        private string? FindColumnValue(IEnumerable<string> requestedLabels)
        {
            var labels = _allLabels.OrderByDescending(x => x.Length).ToList();
            for (var i = 0; i < _lines.Length - 1; i++)
            {
                var matches = labels
                    .Select(label =>
                    {
                        var index = _lines[i].IndexOf(label, StringComparison.OrdinalIgnoreCase);
                        return new { Label = label, Index = index };
                    })
                    .Where(x => x.Index >= 0)
                    .OrderBy(x => x.Index)
                    .GroupBy(x => x.Index)
                    .Select(x => x.First())
                    .ToList();

                if (matches.Count < 2)
                    continue;

                foreach (var requested in requestedLabels)
                {
                    var target = matches.FirstOrDefault(x => string.Equals(x.Label, requested, StringComparison.OrdinalIgnoreCase));
                    if (target is null)
                        continue;

                    var targetIndex = matches.IndexOf(target);
                    var start = target.Index + target.Label.Length;
                    var end = targetIndex + 1 < matches.Count ? matches[targetIndex + 1].Index : _lines[i].Length;
                    var nextLine = _lines[i + 1];

                    if (start >= nextLine.Length)
                        continue;

                    end = Math.Min(end, nextLine.Length);
                    start = Math.Min(start, end);
                    var value = Clean(nextLine[start..end]);
                    if (!string.IsNullOrWhiteSpace(value) && !IsSectionHeading(value))
                        return value;
                }
            }

            return null;
        }

        public string? FindDate(string field)
        {
            var value = Find(field);
            if (!string.IsNullOrWhiteSpace(value) && ParseDate(value).HasValue)
                return value;

            if (!_aliases.TryGetValue(field, out var labels))
                return value;

            // Fallback for PDFs/OCR where the label and value are not kept on
            // adjacent logical lines. Search a small physical-text window for
            // a date immediately following a known DOB label.
            foreach (var label in labels.OrderByDescending(x => x.Length))
            {
                var match = Regex.Match(
                    string.Join(" ", _lines),
                    $@"{Regex.Escape(label)}\s*[:#-]?\s*(?<date>\d{{1,2}}[/-]\d{{1,2}}[/-]\d{{2,4}})",
                    RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups["date"].Value;
            }

            return value;
        }

        public string? FindMultiline(string field, params string[] stopFields)
        {
            if (!_aliases.TryGetValue(field, out var labels)) return null;

            // First try the label/value layout while retaining subsequent lines
            // until another known label or section heading is reached.
            for (var i = 0; i < _lines.Length; i++)
            {
                var line = _lines[i];
                var matchingLabel = labels.OrderByDescending(x => x.Length)
                    .FirstOrDefault(label => string.Equals(line.Trim(), label.Trim(), StringComparison.OrdinalIgnoreCase));

                if (matchingLabel is null)
                    continue;

                var parts = new List<string>();
                for (var j = i + 1; j < _lines.Length && j <= i + 8; j++)
                {
                    if (IsAnyLabelLine(_lines[j]) || IsSectionHeading(_lines[j])) break;
                    if (!string.IsNullOrWhiteSpace(_lines[j])) parts.Add(_lines[j]);
                }

                if (parts.Count > 0)
                    return Clean(string.Join(" ", parts));
            }

            // Then use the normal reader for single-line/table layouts.
            return Find(field);
        }

        public string? FindFullNamePart(int partIndex)
        {
            if (!_aliases.TryGetValue("PatientName", out var labels)) return null;

            string? fullName = null;
            for (var i = 0; i < _lines.Length; i++)
            {
                if (!IsLabelLine(_lines[i], labels)) continue;
                for (var j = i + 1; j < _lines.Length && j <= i + 2; j++)
                {
                    if (IsAnyLabelLine(_lines[j]) || IsSectionHeading(_lines[j])) break;
                    if (!string.IsNullOrWhiteSpace(_lines[j]))
                    {
                        fullName = Clean(_lines[j]);
                        break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(fullName)) break;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                foreach (var label in labels.OrderByDescending(x => x.Length))
                {
                    var match = Regex.Match(
                        string.Join(" ", _lines),
                        $@"{Regex.Escape(label)}\s*[:#-]?\s*(?<name>[A-Za-z][A-Za-z'\-]+(?:\s+[A-Za-z][A-Za-z'\-]+){{1,4}})",
                        RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        fullName = Clean(match.Groups["name"].Value);
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(fullName)) return null;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;
            if (partIndex == 0) return parts[0];
            if (partIndex == -1) return parts.Length > 1 ? parts[^1] : parts[0];
            if (parts.Length >= 3 && partIndex < parts.Length - 1) return parts[partIndex];
            return null;
        }

        public List<string> FindServices(List<string> configuredLabels)
        {
            var result = new List<string>();
            var labels = configuredLabels.Count > 0 ? configuredLabels : ["Ordered Services", "Services", "Service Type", "Home Health Services"];

            foreach (var line in _lines)
            {
                foreach (var label in labels)
                {
                    if (!line.Contains(label, StringComparison.OrdinalIgnoreCase)) continue;
                    var service = new Regex(Regex.Escape(label), RegexOptions.IgnoreCase).Replace(line, "", 1).Trim(' ', ':', '-', '#');
                    if (!string.IsNullOrWhiteSpace(service)) result.Add(Clean(service)!);
                }

                // Table-style rows: detect requested services without requiring
                // a specific clinic's column arrangement.
                var match = Regex.Match(line, @"^(?<service>(?:Skilled Nursing|Physical Therapy|Occupational Therapy|Home Health Aide|Medical Social Worker|SN|PT|OT|HHA|MSW)(?:\s*\([^)]*\))?)\s+(?<flag>■\s*(?:Yes|No)|Yes|No)", RegexOptions.IgnoreCase);
                if (match.Success && Regex.IsMatch(match.Groups["flag"].Value, @"Yes", RegexOptions.IgnoreCase))
                    result.Add(Clean(match.Groups["service"].Value)!);
            }

            return result.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public string? FindSectionNotes()
        {
            var headings = new[] { "Clinical Summary", "Current Symptoms / Risks", "Special Instructions", "Visit Notes" };
            var collected = new List<string>();
            foreach (var heading in headings)
            {
                for (var i = 0; i < _lines.Length; i++)
                {
                    if (!string.Equals(_lines[i].Trim(), heading, StringComparison.OrdinalIgnoreCase)) continue;
                    for (var j = i + 1; j < _lines.Length && j <= i + 6; j++)
                    {
                        if (IsAnyLabelLine(_lines[j])) break;
                        if (!IsSectionHeading(_lines[j])) collected.Add(_lines[j]);
                    }
                    break;
                }
            }
            return collected.Count == 0 ? null : Clean(string.Join(" ", collected));
        }

        private bool IsLabelLine(string line, IEnumerable<string> labels) => labels.Any(label => string.Equals(line.Trim(), label.Trim(), StringComparison.OrdinalIgnoreCase));
        private bool IsAnyLabelLine(string line) => _allLabels.Any(label => string.Equals(line.Trim(), label.Trim(), StringComparison.OrdinalIgnoreCase));
        private static bool IsSectionHeading(string line) => Regex.IsMatch(line.Trim(), @"^\d+\.\s+|^\d+\s+", RegexOptions.IgnoreCase) || line.Trim().Equals("HOME HEALTH CARE PATIENT REFERRAL", StringComparison.OrdinalIgnoreCase);
        private static string? Clean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return Regex.Replace(value.Trim(), @"\s{2,}", " ");
        }
    }
}
