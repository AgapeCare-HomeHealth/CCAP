using CCAP.Web.Common.Models;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Patients.Models;
using CCAP.Web.Features.Tracker.ReferralDrafts.Models;
using CCAP.Web.Features.Tracker.ReferralIntake.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CCAP.Web.Features.Tracker.ReferralIntake.Services;

public sealed class ReferralIntakeService
{
    private const long MaxFileSize =
        10 * 1024 * 1024;

    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public ReferralIntakeService(
        CcapApiClient api,
        MockDataStore mock,
        MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    // =========================================================
    // REFERRAL PDF EXTRACTION
    // =========================================================

    public async Task<ReferralDocumentExtractionDto> ExtractReferralPdfAsync(
        byte[] pdfBytes,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (pdfBytes is null || pdfBytes.Length == 0)
            throw new InvalidOperationException("The referral PDF is empty.");

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(pdfBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "pdf", fileName);

        using var response = await _api.PostMultipartAsync(
            "api/referrals/extract",
            content,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = TryExtractMessage(body);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(message)
                    ? "Unable to extract values from the referral PDF."
                    : message);
        }

        return JsonSerializer.Deserialize<ReferralDocumentExtractionDto>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException(
                "The referral extraction service returned an empty response.");
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<ReferralIntakeResultDto> CreateAsync(
        ReferralIntakeModel model,
        CancellationToken cancellationToken = default)
    {
        // =====================================================
        // VALIDATION
        // =====================================================

        Validate(model);

        // =====================================================
        // MOCK MODE
        // =====================================================

        if (_options.Enabled)
        {
            return CreateMock(model);
        }

        // =====================================================
        // MULTIPART FORM
        // =====================================================

        using var form =
            new MultipartFormDataContent();

        // =====================================================
        // PATIENT
        // =====================================================

        Add(
            form,
            "ReferralDraftId",
            model.ReferralDraftId?.ToString());

        Add(
            form,
            "MRN",
            model.MRN);

        Add(
            form,
            "FirstName",
            model.FirstName);

        Add(
            form,
            "MiddleName",
            model.MiddleName);

        Add(
            form,
            "LastName",
            model.LastName);

        Add(
            form,
            "DateOfBirth",
            model.DateOfBirth?
                .ToString("yyyy-MM-dd"));

        Add(
            form,
            "Gender",
            model.Gender);

        Add(
            form,
            "PrimaryPhone",
            model.PrimaryPhone);

        Add(
            form,
            "AlternatePhone",
            model.AlternatePhone);

        Add(
            form,
            "StreetAddress",
            model.StreetAddress);

        Add(
            form,
            "City",
            model.City);

        Add(
            form,
            "State",
            model.State);

        Add(
            form,
            "ZipCode",
            model.ZipCode);

        // =====================================================
        // EMERGENCY CONTACT
        // =====================================================

        Add(
            form,
            "EmergencyContactName",
            model.EmergencyContactName);

        Add(
            form,
            "EmergencyContactRelationship",
            model.EmergencyContactRelationship);

        Add(
            form,
            "EmergencyContactPhone",
            model.EmergencyContactPhone);

        // =====================================================
        // REFERRAL
        // =====================================================

        Add(
            form,
            "ReferralNumber",
            model.ReferralNumber);

        Add(
            form,
            "ReferralDate",
            model.ReferralDate.ToString("O"));

        Add(
            form,
            "ReferralSource",
            model.ReferralSource);

        Add(
            form,
            "Priority",
            model.Priority);

        // =====================================================
        // INSURANCE
        // =====================================================

        Add(
            form,
            "PrimaryInsurance",
            model.PrimaryInsurance);

        Add(
            form,
            "InsuranceMemberId",
            model.InsuranceMemberId);

        Add(
            form,
            "AuthorizationDate",
            model.AuthorizationDate?
                .ToString("yyyy-MM-dd"));

        Add(
            form,
            "ApprovedVisits",
            model.ApprovedVisits?.ToString());

        Add(
            form,
            "AuthorizationRequired",
            model.AuthorizationRequired.ToString());

        // =====================================================
        // PHYSICIAN
        // =====================================================

        Add(
            form,
            "ReferringPhysician",
            model.ReferringPhysician);

        Add(
            form,
            "PhysicianPhone",
            model.PhysicianPhone);

        // =====================================================
        // CLINICAL
        // =====================================================

        Add(
            form,
            "PrimaryDiagnosis",
            model.PrimaryDiagnosis);

        Add(
            form,
            "SecondaryDiagnosis",
            model.SecondaryDiagnosis);

        Add(
            form,
            "ReferralNotes",
            model.ReferralNotes);

        // =====================================================
        // ASSIGNMENT
        // =====================================================

        Add(
            form,
            "CoordinatorId",
            model.CoordinatorId?.ToString());

        Add(
            form,
            "ClinicianId",
            model.ClinicianId?.ToString());

        Add(
            form,
            "DisciplineId",
            model.DisciplineId?.ToString());

        // =====================================================
        // SCHEDULING
        // =====================================================

        Add(
            form,
            "SocDate",
            model.SocDate?.ToString("yyyy-MM-dd"));

        Add(
            form,
            "VisitPriority",
            model.VisitPriority);

        Add(
            form,
            "CaseStatus",
            model.CaseStatus);

        // =====================================================
        // INTERNAL NOTES
        // =====================================================

        Add(
            form,
            "InternalNotes",
            model.InternalNotes);

        // =====================================================
        // ORDERED SERVICES
        // =====================================================

        if (model.OrderedServices is not null)
        {
            foreach (var service in model.OrderedServices)
            {
                Add(
                    form,
                    "OrderedServices",
                    service);
            }
        }

        // =====================================================
        // PDF METADATA
        // =====================================================
        Add(form, "PdfFileName", model.ReferralPdfFileName);
        Add(form, "PdfContentType", model.ReferralPdfContentType);
        Add(form, "PdfSize", model.ReferralPdfSize?.ToString());

        // =====================================================
        // PDF BYTES
        // =====================================================
        //
        // TEMPORARILY DISABLED.
        //
        // The PDF is intentionally NOT sent to the API.
        //
        // When cloud/hosted file storage is available,
        // restore this block.
        //
        // =====================================================

        /*
        if (model.ReferralPdfBytes is not null &&
            model.ReferralPdfBytes.Length > 0)
        {
            using var fileContent =
                new ByteArrayContent(
                    model.ReferralPdfBytes);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(
                    model.ReferralPdfContentType
                    ?? "application/pdf");

            form.Add(
                fileContent,
                "Pdf",
                model.ReferralPdfFileName
                ?? "referral.pdf");
        }
        */

        // =====================================================
        // SEND TO API
        // =====================================================

        using var response =
            await _api.PostMultipartAsync(
                "api/referrals/intake",
                form,
                cancellationToken);

        var body =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        // =====================================================
        // API ERROR
        // =====================================================

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(body)
                    ? $"Referral creation failed: " +
                      $"{(int)response.StatusCode} " +
                      $"{response.ReasonPhrase}"
                    : body);
        }

        // =====================================================
        // RESPONSE
        // =====================================================

        return
            JsonSerializer.Deserialize<ReferralIntakeResultDto>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? throw new InvalidOperationException(
                "API returned an empty referral creation response.");
    }


    // =========================================================
    // MOCK
    // =========================================================

    private ReferralIntakeResultDto CreateMock(
        ReferralIntakeModel model)
    {
        var patientId =
            Guid.NewGuid();

        var patient =
            new PatientListItem
            {
                PatientId = patientId,

                Name =
                    $"{model.FirstName} " +
                    $"{model.MiddleName} " +
                    $"{model.LastName}"
                        .Replace(
                            "  ",
                            " ")
                        .Trim(),

                MRN =
                    string.IsNullOrWhiteSpace(model.MRN)
                        ? $"MRN-{Guid.NewGuid():N}"
                        : model.MRN,

                Status =
                    "Active",

                PrimaryDiagnosis =
                    model.PrimaryDiagnosis
                    ?? string.Empty,

                AssignedClinician =
                    "Assigned",

                NextVisit =
                    model.SocDate?
                        .ToString("MM/dd/yyyy")
                    ?? string.Empty
            };

        _mock.Patients.Insert(
            0,
            patient);

        return new ReferralIntakeResultDto
        {
            PatientId =
                patientId,

            ReferralId =
                Guid.NewGuid(),

            ReferralNumber =
                string.IsNullOrWhiteSpace(model.ReferralNumber)
                    ? $"REF-{Guid.NewGuid():N}"
                    : model.ReferralNumber,

            // =================================================
            // PDF STORAGE DISABLED
            // =================================================

            ReferralDocumentId =
                null,

            StorageKey =
                null
        };
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private static void Validate(ReferralIntakeModel model)
    {
        // EMR is the source of truth for most referral details.
        // New referral creation therefore requires only the patient's
        // name and date of birth. All other fields are optional and may
        // be completed later from the EMR/referral workflow.
        if (string.IsNullOrWhiteSpace(model.FirstName))
            throw new InvalidOperationException("First name is required.");

        if (string.IsNullOrWhiteSpace(model.LastName))
            throw new InvalidOperationException("Last name is required.");

        if (model.DateOfBirth is null)
            throw new InvalidOperationException("Date of birth is required.");

        if (model.ReferralPdfBytes is not null &&
            model.ReferralPdfBytes.Length > MaxFileSize)
        {
            throw new InvalidOperationException(
                "Referral PDF cannot exceed 10 MB.");
        }
    }


    // =========================================================
    // PHONE VALIDATION
    // =========================================================

    private static void ValidatePhone(
        string? phone,
        string fieldName,
        bool required)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            if (required)
            {
                throw new InvalidOperationException(
                    $"{fieldName} is required.");
            }

            return;
        }

        // Numbers only
        if (!phone.All(char.IsDigit))
        {
            throw new InvalidOperationException(
                $"{fieldName} must contain numbers only.");
        }

        // 10-15 digits
        if (phone.Length < 10 ||
            phone.Length > 15)
        {
            throw new InvalidOperationException(
                $"{fieldName} must contain between 10 and 15 digits.");
        }
    }


    // =========================================================
    // FORM HELPER
    // =========================================================

    private static void Add(
        MultipartFormDataContent form,
        string name,
        string? value)
    {
        if (value is null)
            return;

        form.Add(
            new StringContent(value),
            name);
    }

    private static string? TryExtractMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("message", out var message))
                return message.GetString();
        }
        catch (JsonException)
        {
            // Fall back to the raw response below.
        }

        return body;
    }

    // =========================================================
    // SAVE DRAFT
    // =========================================================

    public async Task<SaveReferralDraftResultDto> SaveDraftAsync(
        ReferralIntakeModel model,
        CancellationToken cancellationToken = default)
    {
        var draftData = ToDraftData(model);

        var request = new
        {
            ReferralDraftId = model.ReferralDraftId,
            Data = JsonSerializer.Serialize(draftData)
        };

        using var response = await _api.PostAsJsonAsync(
            "api/referrals/draft",
            request,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(body)
                    ? "Unable to save referral draft."
                    : body);
        }

        var result = JsonSerializer.Deserialize<SaveReferralDraftResultDto>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result is null)
            throw new InvalidOperationException("API returned an empty draft response.");

        model.ReferralDraftId = result.ReferralDraftId;
        model.ReferralPdfFileName = draftData.ReferralPdfFileName;
        model.ReferralPdfContentType = draftData.ReferralPdfContentType;
        model.ReferralPdfSize = draftData.ReferralPdfSize;

        return result;
    }

    private static ReferralIntakeDraftData ToDraftData(ReferralIntakeModel model)
        => new()
        {
            ReferralPdfFileName = model.ReferralPdfFileName,
            ReferralPdfContentType = model.ReferralPdfContentType,
            ReferralPdfSize = model.ReferralPdfSize,
            MRN = model.MRN,
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            LastName = model.LastName,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            PrimaryPhone = model.PrimaryPhone,
            AlternatePhone = model.AlternatePhone,
            StreetAddress = model.StreetAddress,
            City = model.City,
            State = model.State,
            ZipCode = model.ZipCode,
            EmergencyContactName = model.EmergencyContactName,
            EmergencyContactRelationship = model.EmergencyContactRelationship,
            EmergencyContactPhone = model.EmergencyContactPhone,
            ReferralNumber = model.ReferralNumber,
            ReferralDate = model.ReferralDate,
            ReferralSource = model.ReferralSource,
            Priority = model.Priority,
            PrimaryInsurance = model.PrimaryInsurance,
            InsuranceMemberId = model.InsuranceMemberId,
            AuthorizationDate = model.AuthorizationDate,
            ApprovedVisits = model.ApprovedVisits,
            AuthorizationRequired = model.AuthorizationRequired,
            ReferringPhysician = model.ReferringPhysician,
            PhysicianPhone = model.PhysicianPhone,
            PrimaryDiagnosis = model.PrimaryDiagnosis,
            SecondaryDiagnosis = model.SecondaryDiagnosis,
            OrderedServices = model.OrderedServices.ToList(),
            ReferralNotes = model.ReferralNotes,
            CoordinatorId = model.CoordinatorId,
            ClinicianId = model.ClinicianId,
            DisciplineId = model.DisciplineId,
            SocDate = model.SocDate,
            VisitPriority = model.VisitPriority,
            CaseStatus = model.CaseStatus,
            InternalNotes = model.InternalNotes
        };

    // =========================================================
    // GET DRAFT
    // =========================================================
    public async Task<PagedResult<ReferralDraftListItem>>
    GetDraftsAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string sortBy = "UpdatedAt",
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/referrals/drafts" +
            $"?pageNumber={pageNumber}" +
            $"&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url +=
                $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        url += $"&sortBy={Uri.EscapeDataString(sortBy)}&sortDescending={sortDescending.ToString().ToLowerInvariant()}";

        return await _api.GetFromJsonAsync<
            PagedResult<ReferralDraftListItem>>(
            url,
            cancellationToken)
            ?? new PagedResult<ReferralDraftListItem>();
    }

    public async Task<ReferralIntakeModel> LoadDraftAsync(
    Guid draftId,
    CancellationToken cancellationToken = default)
    {
        var json =
            await _api.GetFromJsonAsync<ReferralDraftDetails>(
                $"api/referrals/drafts/{draftId}",
                cancellationToken);

        if (json is null ||
            string.IsNullOrWhiteSpace(json.Data))
        {
            throw new InvalidOperationException(
                "Referral draft was not found.");
        }

        var draftData =
            JsonSerializer.Deserialize<ReferralIntakeDraftData>(
                json.Data,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (draftData is null)
        {
            throw new InvalidOperationException(
                "The referral draft data could not be loaded.");
        }

        var model = new ReferralIntakeModel
        {
            ReferralDraftId = draftId,
            ReferralPdfFileName = draftData.ReferralPdfFileName,
            ReferralPdfContentType = draftData.ReferralPdfContentType,
            ReferralPdfSize = draftData.ReferralPdfSize,
            MRN = draftData.MRN,
            FirstName = draftData.FirstName,
            MiddleName = draftData.MiddleName,
            LastName = draftData.LastName,
            DateOfBirth = draftData.DateOfBirth,
            Gender = draftData.Gender,
            PrimaryPhone = draftData.PrimaryPhone,
            AlternatePhone = draftData.AlternatePhone,
            StreetAddress = draftData.StreetAddress,
            City = draftData.City,
            State = draftData.State,
            ZipCode = draftData.ZipCode,
            EmergencyContactName = draftData.EmergencyContactName,
            EmergencyContactRelationship = draftData.EmergencyContactRelationship,
            EmergencyContactPhone = draftData.EmergencyContactPhone,
            ReferralNumber = draftData.ReferralNumber,
            ReferralDate = draftData.ReferralDate,
            ReferralSource = draftData.ReferralSource,
            Priority = draftData.Priority,
            PrimaryInsurance = draftData.PrimaryInsurance,
            InsuranceMemberId = draftData.InsuranceMemberId,
            AuthorizationDate = draftData.AuthorizationDate,
            ApprovedVisits = draftData.ApprovedVisits,
            AuthorizationRequired = draftData.AuthorizationRequired,
            ReferringPhysician = draftData.ReferringPhysician,
            PhysicianPhone = draftData.PhysicianPhone,
            PrimaryDiagnosis = draftData.PrimaryDiagnosis,
            SecondaryDiagnosis = draftData.SecondaryDiagnosis,
            OrderedServices = draftData.OrderedServices,
            ReferralNotes = draftData.ReferralNotes,
            CoordinatorId = draftData.CoordinatorId,
            ClinicianId = draftData.ClinicianId,
            DisciplineId = draftData.DisciplineId,
            SocDate = draftData.SocDate,
            VisitPriority = draftData.VisitPriority,
            CaseStatus = draftData.CaseStatus,
            InternalNotes = draftData.InternalNotes
        };

        return model;
    }

}