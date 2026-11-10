using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CCAP.Application.Abstractions.Referrals;
using CCAP.Application.Features.Referrals.DTOs;
using Microsoft.Extensions.Configuration;
using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using System.Runtime.InteropServices;
using RapidOcrNet;
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

    /// <summary>
    /// Digital-text fallback for callers that still submit the original PDF to the API.
    /// The interactive Referral Intake now performs scanned-page OCR in the browser and
    /// submits page text through ExtractFromTextAsync, so the API requires no native OCR
    /// executable or machine-specific path.
    /// </summary>
    public async Task<ReferralDocumentExtractionResult> ExtractAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pdfStream);

        await using var buffer = new MemoryStream();
        await pdfStream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        if (bytes.Length == 0)
            throw new ArgumentException("The referral PDF is empty.", nameof(pdfStream));

        var pageTexts = new List<string>();
        using (var document = PdfDocument.Open(bytes))
        {
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var positioned = BuildPositionedPageText(page);
                pageTexts.Add(!string.IsNullOrWhiteSpace(positioned)
                    ? positioned
                    : page.Text ?? string.Empty);
                if (pageTexts.Count > 25)
                    throw new InvalidOperationException("Referral PDFs may contain no more than 25 pages.");
            }
        }

        return await ExtractFromTextAsync(pageTexts, ocrUsed: false, confidence: null, cancellationToken);
    }

    public Task<ReferralDocumentExtractionResult> ExtractFromTextAsync(
        IReadOnlyList<string> pageTexts,
        bool ocrUsed,
        double? confidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pageTexts);
        cancellationToken.ThrowIfCancellationRequested();

        if (pageTexts.Count is < 1 or > 25)
            throw new InvalidOperationException("Referral extraction requires between 1 and 25 pages.");

        var normalizedPages = pageTexts.Select(text => text ?? string.Empty).ToList();
        var combinedText = string.Join(
            Environment.NewLine + Environment.NewLine,
            normalizedPages.Select((text, index) =>
                string.Concat(Environment.NewLine, "[[PAGE ", index + 1, "]]", Environment.NewLine, text)));

        if (combinedText.Length > 1_500_000)
            throw new InvalidOperationException("The extracted referral text is too large to process safely.");

        var textValue = digitalText.ToString();
        double? confidence = null;
        var profiles = ReferralFormProfileLoader.Load(_configuration);
        var result = ReferralDocumentFieldParser.Parse(combinedText, ocrUsed, confidence, profiles);

        // Second pass: when embedded page images exist, run OCR even if the
        // PDF text layer returned some values. Scanned referrals can contain a
        // small amount of selectable text alongside image-only sections, so
        // using a "fewer than two fields" threshold silently misses those fields.
        // MergeMissing preserves the text-layer values and fills only gaps from OCR.
        if (imagePayloads.Count > 0)
        {
            // Do not silently report "text layer read directly" when OCR failed.
            // The upload UI catches this exception and can show a useful message.
            var ocr = await RunOcrAsync(imagePayloads, cancellationToken);
            if (!string.IsNullOrWhiteSpace(ocr.Text))
            {
                var ocrResult = ReferralDocumentFieldParser.Parse(ocr.Text, true, ocr.Confidence, profiles);
                result = ReferralDocumentFieldParser.MergeMissing(result, ocrResult);
                confidence = ocr.Confidence;
            }
            else if (ReferralDocumentFieldParser.CountDetectedValues(result) == 0)
            {
                throw new InvalidOperationException(
                    "RapidOCR did not recognize readable text in the PDF pages. " +
                    "Check image quality, page rotation, and the published OCR model files.");
            }
        }
        else if (ReferralDocumentFieldParser.CountDetectedValues(result) == 0)
        {
            throw new InvalidOperationException(
                $"CCAP could not extract referral values. {renderingFailure ?? "No OCR-ready page images were produced."} " +
                "Verify the PDFium/Docnet native runtime files and the RapidOCR model files in the published API.");
        }

        result = ReferralExtractionSafety.Apply(result, normalizedPages);
        result.DetectedFieldCount = ReferralDocumentFieldParser.CountDetectedValues(result);
        return Task.FromResult(result);
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

    private static List<byte[]> RenderPdfPagesToPng(byte[] pdfBytes, CancellationToken cancellationToken)
    {
        var rendered = new List<byte[]>();
        using var document = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(1700, 2300));
        var pageCount = document.GetPageCount();

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var page = document.GetPageReader(pageIndex);
            var width = page.GetPageWidth();
            var height = page.GetPageHeight();
            var pixels = page.GetImage();
            if (width <= 0 || height <= 0 || pixels.Length < width * height * 4)
                continue;

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var bitmap = new SKBitmap(info);
            Marshal.Copy(pixels, 0, bitmap.GetPixels(), Math.Min(pixels.Length, height * bitmap.RowBytes));
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 88);
            rendered.Add(data.ToArray());
        }

        return rendered;
    }

    private Task<(string Text, double Confidence)> RunOcrAsync(
        IReadOnlyCollection<byte[]> images,
        CancellationToken cancellationToken)
    {
        var text = new StringBuilder();
        var confidenceTotal = 0d;
        var confidenceCount = 0;

        // Resolve model files from the API application's base directory explicitly.
        // RapidOcr.InitModels() uses relative paths (models\v5\...), which depend
        // on the process working directory and can fail under Visual Studio/IIS/Plesk
        // even when the model folder exists beside the application binaries.
        var modelDirectory = Path.Combine(AppContext.BaseDirectory, "models", "v5");
        var detectorModel = Path.Combine(modelDirectory, "ch_PP-OCRv5_mobile_det.onnx");
        var classifierModel = Path.Combine(modelDirectory, "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx");
        var recognizerModel = Path.Combine(modelDirectory, "latin_PP-OCRv5_rec_mobile_infer.onnx");
        var dictionaryFile = Path.Combine(modelDirectory, "ppocrv5_latin_dict.txt");

        var missingModels = new[] { detectorModel, classifierModel, recognizerModel, dictionaryFile }
            .Where(path => !File.Exists(path))
            .ToArray();
        if (missingModels.Length > 0)
        {
            throw new FileNotFoundException(
                $"RapidOCR model files were not found under '{modelDirectory}'. Missing: " +
                string.Join(", ", missingModels.Select(Path.GetFileName)) +
                $". AppContext.BaseDirectory is '{AppContext.BaseDirectory}'. Ensure the complete API output, including models\v5, is present.");
        }

        using var engine = new RapidOcr();
        engine.InitModels(detectorModel, classifierModel, recognizerModel, dictionaryFile);

        foreach (var imageBytes in images)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bitmap = SKBitmap.Decode(imageBytes);
            if (bitmap is null || bitmap.Width <= 0 || bitmap.Height <= 0)
                continue;

            var pageResult = engine.Detect(bitmap, RapidOcrOptions.Default);
            if (pageResult is null)
                continue;

            if (!string.IsNullOrWhiteSpace(pageResult.StrRes))
                text.AppendLine(pageResult.StrRes);

            foreach (var block in pageResult.TextBlocks)
            {
                if (block.CharScores is not null && block.CharScores.Any())
                {
                    confidenceTotal += block.CharScores.Average();
                    confidenceCount++;
                }
            }
        }

        return Task.FromResult((
            text.ToString(),
            confidenceCount == 0 ? 0 : confidenceTotal / confidenceCount));
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
            ["EmergencyContactName"] = ["Emergency Contact Name", "Contact Person", "Next of Kin Name", "Emergency Contact", "Next of Kin"],
            ["EmergencyContactRelationship"] = ["Emergency Contact Relationship", "Contact Relationship", "Relationship to Patient", "Relationship", "Relation"],
            ["EmergencyContactPhone"] = ["Emergency Contact Phone", "Emergency Contact Number", "Contact Phone", "Emergency Phone", "Next of Kin Phone"],
            ["ReferralNumber"] = ["Referral Number", "Referral #", "Referral ID", "Referral No", "Referral Number/ID", "Referral ID/No"],
            ["ReferralDate"] = ["Referral Date", "Date Referred", "Date of Referral", "Date Received", "Order Date"],
            ["ReferralSource"] = ["Referral Source", "Referred By", "Referral From", "Referral Facility", "Referring Facility", "Referral Agency", "Facility", "Home Health Agency"],
            ["Priority"] = ["Priority", "Referral Priority", "Urgency"],
            ["PrimaryInsurance"] = ["Primary Insurance", "Insurance Company", "Insurance Provider", "Payer", "Payor", "Primary Payer", "Primary Payor", "Primary Insurance Plan"],
            ["InsuranceMemberId"] = ["Insurance Member ID", "Member ID", "Member Number", "Subscriber ID", "Policy Number", "Policy ID", "Insurance ID", "Member / Policy ID"],
            ["AuthorizationDate"] = ["Authorization Date", "Auth Date"],
            ["ApprovedVisits"] = ["Approved Visits", "Visits Approved", "Authorized Visits"],
            ["ReferringPhysician"] = ["Referring Physician", "Referring Provider", "Ordering Physician", "Ordering Provider", "Ordering MD", "Physician Ordering", "Provider Name", "Ordered By"],
            ["PhysicianPhone"] = ["Referring Physician Phone", "Physician Phone", "Provider Phone", "Doctor Phone", "Ordering Provider Phone", "Provider Telephone"],
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
        var streetAddress = CleanAddress(parser.FindMultiline("StreetAddress", "City", "State", "ZipCode"))
            ?? FindStreetAddressFromDocument(rawText);
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

        // Emergency contacts must be read from their own local section rather than
        // from generic "Name", "Relationship", or "Phone" labels elsewhere in a packet.
        var emergencyContact = FindEmergencyContact(rawText);
        var emergencyContactName = emergencyContact.Name ?? NormalizePersonName(parser.Find("EmergencyContactName"));
        var emergencyContactRelationship = emergencyContact.Relationship ?? NormalizeRelationship(parser.Find("EmergencyContactRelationship"));
        var emergencyContactPhone = NormalizePhone(emergencyContact.Phone) ?? emergencyPhone;

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
            EmergencyContactName = IsPlausibleName(emergencyContactName) ? emergencyContactName : null,
            EmergencyContactRelationship = emergencyContactRelationship,
            EmergencyContactPhone = emergencyContactPhone,
            ReferralNumber = parser.Find("ReferralNumber"),
            ReferralDate = referralDate,
            ReferralSource = parser.Find("ReferralSource"),
            Priority = parser.Find("Priority"),
            PrimaryInsurance = parser.Find("PrimaryInsurance"),
            InsuranceMemberId = parser.Find("InsuranceMemberId"),
            AuthorizationDate = authorizationDate,
            ApprovedVisits = approvedVisits,
            ReferringPhysician = FindPrimaryPhysician(rawText, parser),
            PhysicianPhone = NormalizePhone(parser.Find("PhysicianPhone")),
            PrimaryDiagnosis = parser.Find("PrimaryDiagnosis"),
            SecondaryDiagnosis = parser.Find("SecondaryDiagnosis"),
            OrderedServices = orderedServices,
            ReferralNotes = parser.Find("ReferralNotes") ?? parser.FindSectionNotes()
        };

        result.DetectedFieldCount = CountDetectedValues(result);
        return result;
    }

    private static (string? Name, string? Relationship, string? Phone) FindEmergencyContact(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return (null, null, null);

        var lines = rawText.Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim().Trim('|'))
            .Where(x => x.Length > 0)
            .ToArray();

        // RapidOCR can return the same table row as one line, separate columns,
        // or several adjacent lines. Search locally around the emergency-contact
        // marker instead of requiring literal pipe characters.
        for (var i = 0; i < lines.Length; i++)
        {
            if (!Regex.IsMatch(lines[i], @"\bEmergency\s+Contact\s*#?\s*\d*\b", RegexOptions.IgnoreCase))
                continue;

            var windowStart = Math.Max(0, i - 2);
            var windowEnd = Math.Min(lines.Length - 1, i + 4);
            var window = string.Join(" | ", lines.Skip(windowStart).Take(windowEnd - windowStart + 1));

            // Most common scanned table layout: "Last, First | Emergency Contact #1 | Son | Home: phone".
            var commaName = Regex.Match(window,
                @"(?<name>[A-Z][A-Za-z'’.-]{1,30},\s*[A-Z][A-Za-z'’.-]{1,30}(?:\s+[A-Z])?)\s*(?:\||\s)+(?=Emergency\s+Contact\s*#?\s*\d*)",
                RegexOptions.IgnoreCase);
            string? name = commaName.Success ? NormalizePersonName(commaName.Groups["name"].Value) : null;

            // OCR sometimes removes punctuation/columns: capture a name immediately before the label.
            if (name is null)
            {
                var beforeLabel = Regex.Match(window,
                    @"(?<name>[A-Z][A-Za-z'’.-]{1,30}(?:\s+[A-Z][A-Za-z'’.-]{1,30}){1,2})\s+Emergency\s+Contact\s*#?\s*\d*",
                    RegexOptions.IgnoreCase);
                if (beforeLabel.Success && !Regex.IsMatch(beforeLabel.Groups["name"].Value,
                    @"\b(?:patient|contact|emergency|name|phone|home|work)\b", RegexOptions.IgnoreCase))
                    name = NormalizePersonName(beforeLabel.Groups["name"].Value);
            }

            // If OCR split the person's name onto the line immediately before the marker,
            // accept only a plausible name (never the numeric contact label).
            if (name is null && i > 0)
            {
                var previous = lines[i - 1].Trim('|', ':', '-', ' ');
                if (previous.Contains(',') && IsPlausibleName(NormalizePersonName(previous)))
                    name = NormalizePersonName(previous);
            }

            var relationshipMatch = Regex.Match(window,
                @"\b(Spouse|Wife|Husband|Daughter|Son|Mother|Father|Sister|Brother|Relative|Friend|Caregiver|Guardian|Other)\b",
                RegexOptions.IgnoreCase);
            var relationship = relationshipMatch.Success ? NormalizeRelationship(relationshipMatch.Value) : null;
            var phoneMatch = Regex.Match(window,
                @"(?:\+?1[ .-]?)?(?:\(?\d{3}\)?[ .-]?)\d{3}[ .-]?\d{4}");
            var phone = phoneMatch.Success ? NormalizePhone(phoneMatch.Value) : null;

            if (IsPlausibleName(name) || relationship is not null || phone is not null)
                return (IsPlausibleName(name) ? name : null, relationship, phone);
        }

        // Label/value forms such as "Emergency Contact Name: Jane Doe".
        for (var i = 0; i < lines.Length; i++)
        {
            if (!Regex.IsMatch(lines[i], @"\b(?:emergency contact|next of kin)\b", RegexOptions.IgnoreCase))
                continue;
            var windowLines = lines.Skip(i).Take(8).ToArray();
            var name = FindContactValue(windowLines, @"(?:emergency contact name|contact person|next of kin name)");
            var relationship = NormalizeRelationship(FindContactValue(windowLines,
                @"(?:emergency contact relationship|contact relationship|relationship to patient|relationship|relation)"));
            var phone = NormalizePhone(FindContactValue(windowLines,
                @"(?:emergency contact phone|emergency contact number|contact phone|emergency phone|next of kin phone|phone|telephone|mobile)"));
            if (IsPlausibleName(name) || relationship is not null || phone is not null)
                return (IsPlausibleName(name) ? NormalizePersonName(name) : null, relationship, phone);
        }

        return (null, null, null);
    }

    private static string? NormalizePersonName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var cleaned = Regex.Replace(value.Trim().Trim('|', ':', ';'), @"\s+", " ");
        // Convert "Last, First Middle" into the conventional "First Middle Last".
        var comma = cleaned.Split(',', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (comma.Length == 2 && IsPlausibleName(comma[0]) && IsPlausibleName(comma[1]))
            return comma[1] + " " + comma[0];
        return cleaned;
    }

    private static string? FindPrimaryPhysician(string rawText, ReferralFieldReader parser)
    {
        // Prefer explicitly labelled ordering/referring-provider fields, not arbitrary
        // mentions of physicians or names appearing in clinical notes.
        var lines = rawText.Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim())
            .Where(x => x.Length > 0).ToArray();

        var labels = new[]
        {
            @"referring physician", @"referring provider", @"ordering physician",
            @"ordering provider", @"physician ordering", @"provider name", @"ordered by"
        };
        foreach (var line in lines)
        {
            foreach (var label in labels)
            {
                var match = Regex.Match(line,
                    @"\b" + label + @"\b\s*[:#|\-]?\s*(?<value>[A-Za-z][A-Za-z .,'’\-]{2,80})",
                    RegexOptions.IgnoreCase);
                if (!match.Success) continue;
                var value = Regex.Replace(match.Groups["value"].Value,
                    @"\s+(?:NPI|PHONE|FAX|TEL|TELEPHONE|ADDRESS|DATE|DOB)\b.*$", "", RegexOptions.IgnoreCase).Trim(' ', ',', '|', ':', ';', '-');
                if (LooksLikeProviderName(value)) return NormalizePersonName(value);
            }
        }

        // Fall back only to the explicitly referring/ordering aliases. Avoid the
        // broad generic "Physician", "Doctor", and "Attending Physician" aliases.
        return CleanOptional(parser.Find("ReferringPhysician"));
    }

    private static bool LooksLikeProviderName(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 80 &&
        Regex.IsMatch(value, @"^[A-Za-z][A-Za-z'’.,\-]*(?:\s+[A-Za-z][A-Za-z'’.,\-]*){1,5}$") &&
        !Regex.IsMatch(value, @"\b(?:office|phone|fax|address|npi|date|patient|home health|agency|clinic|facility)\b", RegexOptions.IgnoreCase);

    private static string? FindContactValue(string[] lines, string labelPattern)
    {
        var label = new Regex(@"^\s*(?:" + labelPattern + @")\s*(?:\([^)]*\))?\s*[:#\-]\s*(?<value>.+?)\s*$",
            RegexOptions.IgnoreCase);

        for (var i = 0; i < lines.Length; i++)
        {
            var match = label.Match(lines[i]);
            if (match.Success)
            {
                var value = CleanContactValue(match.Groups["value"].Value);
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

            // For label-only lines, accept the next line only when it doesn't
            // look like another field label or section heading.
            if (Regex.IsMatch(lines[i], @"^\s*(?:" + labelPattern + @")\s*(?:\([^)]*\))?\s*$", RegexOptions.IgnoreCase)
                && i + 1 < lines.Length
                && !Regex.IsMatch(lines[i + 1], @"^(?:name|relationship|relation|phone|telephone|mobile|emergency contact|next of kin|patient|physician|insurance|referral|diagnosis)\b", RegexOptions.IgnoreCase))
            {
                var value = CleanContactValue(lines[i + 1]);
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
        }

        return null;
    }

    private static string? CleanContactValue(string value)
    {
        var cleaned = value.Trim().Trim(':', '-', '|', ';');
        cleaned = Regex.Replace(cleaned, @"\s+(?:Relationship|Relation|Phone|Telephone|Mobile)\s*[:#-]?.*$", "", RegexOptions.IgnoreCase).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
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
        // Remove surrounding OCR labels and keep a phone-shaped sequence only.
        var match = Regex.Match(value, @"(?<!\d)(?:\+?1[ .-]?)?(?:\(?\d{3}\)?[ .-]?)\d{3}[ .-]?\d{4}(?!\d)");
        if (match.Success)
        {
            var digits = Regex.Replace(match.Value, @"\D", "");
            if (digits.Length >= 10 && digits.Length <= 11) return match.Value.Trim();
        }
        var allDigits = Regex.Replace(value, @"\D", "");
        return allDigits.Length >= 7 && allDigits.Length <= 15 && !Regex.IsMatch(value, @"[A-Za-z]{3,}")
            ? value.Trim() : null;
    }

    private static string? FindStreetAddressFromDocument(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return null;

        // OCR and selectable text often separate the street and city/state/ZIP
        // onto adjacent lines. Search the complete document for address-shaped
        // lines instead of relying only on a generic "Address" label match.
        var lines = rawText.Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Regex.Replace(x, @"\s+", " ").Trim().Trim('|', ':', ';'))
            .Where(x => x.Length > 0)
            .ToArray();

        var streetSuffix = @"(?:street|st\.?|avenue|ave\.?|road|rd\.?|boulevard|blvd\.?|drive|dr\.?|lane|ln\.?|court|ct\.?|circle|cir\.?|highway|hwy\.?|parkway|pkwy\.?|way|terrace|ter\.?|place|pl\.?|trail|trl\.?|apt\.?|apartment|unit|suite|ste\.?)";
        var candidates = new List<(string Value, int Score)>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            // Exclude identifiers and other administrative content that can be
            // mistakenly captured by broad aliases (e.g. "UPIN NPI").
            if (Regex.IsMatch(line, @"\b(?:UPIN|NPI|MRN|TIN|FAX|PHONE|TELEPHONE|INSURANCE|MEMBER ID|POLICY|DIAGNOSIS)\b", RegexOptions.IgnoreCase))
                continue;

            var hasStreetNumber = Regex.IsMatch(line, @"^\d{1,6}\s+[A-Za-z]", RegexOptions.IgnoreCase);
            var hasStreetType = Regex.IsMatch(line, @"\b" + streetSuffix + @"\b", RegexOptions.IgnoreCase);
            var hasZip = Regex.IsMatch(line, @"\b\d{5}(?:-\d{4})?\b");
            var hasCityStateZip = Regex.IsMatch(line, @"\b[A-Z]{2}\s+\d{5}(?:-\d{4})?\b", RegexOptions.IgnoreCase);

            string candidate = line;
            var score = 0;
            if (hasStreetNumber) score += 3;
            if (hasStreetType) score += 3;
            if (hasZip) score += 2;
            if (hasCityStateZip) score += 2;

            // Combine a street line with the following city/state/ZIP line.
            if (i + 1 < lines.Length && hasStreetNumber && !hasZip &&
                (Regex.IsMatch(lines[i + 1], @"\b[A-Z]{2}\s+\d{5}(?:-\d{4})?\b", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(lines[i + 1], @"\b\d{5}(?:-\d{4})?\b")))
            {
                candidate = line + ", " + lines[i + 1];
                score += 3;
            }

            // Require multiple address characteristics to avoid populating an
            // address from unrelated numbers or labels.
            if (hasStreetNumber && (hasStreetType || hasZip))
                candidates.Add((candidate, score));
        }

        var best = candidates.OrderByDescending(x => x.Score).FirstOrDefault();
        return string.IsNullOrWhiteSpace(best.Value) ? null : CleanAddress(best.Value);
    }

    private static string? CleanAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var cleaned = value.Trim().Trim(':', '-', '|');
        if (Regex.IsMatch(cleaned, @"^(?:address|street address|home address|patient address)$", RegexOptions.IgnoreCase)) return null;
        if (Regex.IsMatch(cleaned, @"\b(?:UPIN|NPI|MRN|TIN|FAX|PHONE|TELEPHONE|MEMBER ID|POLICY NUMBER)\b", RegexOptions.IgnoreCase)) return null;
        if (!Regex.IsMatch(cleaned, @"\d") && cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2) return null;
        return cleaned;
    }

    private static string? NormalizeRelationship(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var cleaned = value.Trim().Trim(':', '-', '|');
        var allowed = new[] { "spouse", "wife", "husband", "daughter", "son", "mother", "father", "sister", "brother", "relative", "friend", "caregiver", "guardian", "other", "self", "niece", "nephew", "granddaughter", "grandson" };
        return allowed.Contains(cleaned, StringComparer.OrdinalIgnoreCase) ? cleaned : null;
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
        if (!string.IsNullOrWhiteSpace(result.MRN) || result.MrnCandidates.Count > 0) count++;
        if (!string.IsNullOrWhiteSpace(result.FirstName)) count++;
        if (!string.IsNullOrWhiteSpace(result.LastName)) count++;
        if (!string.IsNullOrWhiteSpace(result.MiddleName)) count++;
        if (result.DateOfBirth.HasValue) count++;
        if (!string.IsNullOrWhiteSpace(result.Gender)) count++;
        if (!string.IsNullOrWhiteSpace(result.PrimaryPhone)) count++;
        if (!string.IsNullOrWhiteSpace(result.StreetAddress)) count++;
        if (!string.IsNullOrWhiteSpace(result.City)) count++;
        if (!string.IsNullOrWhiteSpace(result.State)) count++;
        if (!string.IsNullOrWhiteSpace(result.ZipCode)) count++;
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
