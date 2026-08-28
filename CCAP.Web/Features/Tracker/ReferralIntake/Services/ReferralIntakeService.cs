using System.Net.Http.Headers;
using System.Text.Json;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Patients.Models;
using CCAP.Web.Features.Tracker.ReferralIntake.Models;

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
        // PDF
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
                    model.MRN,

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
                model.ReferralNumber,

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

    private static void Validate(
        ReferralIntakeModel model)
    {
        // =====================================================
        // PATIENT
        // =====================================================

        if (string.IsNullOrWhiteSpace(
                model.MRN))
        {
            throw new InvalidOperationException(
                "MRN is required.");
        }

        if (string.IsNullOrWhiteSpace(
                model.FirstName))
        {
            throw new InvalidOperationException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(
                model.LastName))
        {
            throw new InvalidOperationException(
                "Last name is required.");
        }

        // =====================================================
        // PHONE NUMBERS
        // =====================================================

        ValidatePhone(
            model.PrimaryPhone,
            "Primary phone",
            required: true);

        ValidatePhone(
            model.AlternatePhone,
            "Alternate phone",
            required: false);

        ValidatePhone(
            model.EmergencyContactPhone,
            "Emergency contact phone",
            required: false);

        ValidatePhone(
            model.PhysicianPhone,
            "Physician phone",
            required: false);

        // =====================================================
        // ZIP CODE
        // =====================================================

        if (!string.IsNullOrWhiteSpace(
                model.ZipCode))
        {
            if (!model.ZipCode.All(
                    char.IsDigit))
            {
                throw new InvalidOperationException(
                    "ZIP code must contain numbers only.");
            }

            if (model.ZipCode.Length != 5)
            {
                throw new InvalidOperationException(
                    "ZIP code must contain exactly 5 digits.");
            }
        }

        // =====================================================
        // REFERRAL
        // =====================================================

        if (string.IsNullOrWhiteSpace(
                model.ReferralNumber))
        {
            throw new InvalidOperationException(
                "Referral number is required.");
        }

        if (model.ReferralDate == default)
        {
            throw new InvalidOperationException(
                "Referral date is required.");
        }

        if (model.ReferralDate.Date >
            DateTime.Today)
        {
            throw new InvalidOperationException(
                "Referral date cannot be in the future.");
        }

        // =====================================================
        // INSURANCE
        // =====================================================

        if (model.AuthorizationRequired &&
            string.IsNullOrWhiteSpace(
                model.InsuranceMemberId))
        {
            throw new InvalidOperationException(
                "Insurance member ID is required when authorization is required.");
        }

        // =====================================================
        // PHYSICIAN
        // =====================================================

        if (string.IsNullOrWhiteSpace(
                model.ReferringPhysician))
        {
            throw new InvalidOperationException(
                "Referring physician is required.");
        }

        // =====================================================
        // CLINICAL
        // =====================================================

        if (string.IsNullOrWhiteSpace(
                model.PrimaryDiagnosis))
        {
            throw new InvalidOperationException(
                "Primary diagnosis is required.");
        }

        if (model.OrderedServices is null ||
            model.OrderedServices.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one ordered service is required.");
        }

        // =====================================================
        // ASSIGNMENT
        // =====================================================

        if (model.CoordinatorId is null ||
            model.CoordinatorId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Care Coordinator is required.");
        }

        if (model.DisciplineId is null ||
            model.DisciplineId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Discipline is required.");
        }

        // =====================================================
        // SOC
        // =====================================================

        if (model.SocDate is null)
        {
            throw new InvalidOperationException(
                "SOC date is required.");
        }

        if (model.SocDate.Value <
            DateOnly.FromDateTime(
                DateTime.Today))
        {
            throw new InvalidOperationException(
                "SOC date cannot be in the past.");
        }

        // =====================================================
        // CASE STATUS
        // =====================================================

        if (string.IsNullOrWhiteSpace(
                model.CaseStatus))
        {
            throw new InvalidOperationException(
                "Case status is required.");
        }

        // =====================================================
        // PDF VALIDATION
        // =====================================================
        //
        // TEMPORARILY DISABLED.
        //
        // PDF is optional and is NOT sent to the API.
        //
        // When file storage is ready, restore this block.
        //
        // =====================================================

        /*
        if (model.ReferralPdfBytes is not null &&
            model.ReferralPdfBytes.Length > 0)
        {
            if (model.ReferralPdfBytes.Length >
                MaxFileSize)
            {
                throw new InvalidOperationException(
                    "Referral PDF cannot exceed 10 MB.");
            }

            if (string.IsNullOrWhiteSpace(
                    model.ReferralPdfFileName))
            {
                throw new InvalidOperationException(
                    "Referral PDF file name is missing.");
            }

            if (!model.ReferralPdfFileName.EndsWith(
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only PDF files are accepted.");
            }
        }
        */
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
}