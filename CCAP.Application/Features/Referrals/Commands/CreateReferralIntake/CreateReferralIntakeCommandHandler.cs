using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Storage;
using CCAP.Domain.Entities;
using CCAP.Domain.Enums;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;

public sealed class CreateReferralIntakeCommandHandler
    : IRequestHandler<
        CreateReferralIntakeCommand,
        CreateReferralIntakeResult>
{
    private readonly IPatientRepository _patients;
    private readonly IReferralRepository _referrals;
    private readonly ILocationRepository _locations;
    private readonly IServiceTypeRepository _serviceTypes;
    private readonly IPatientTaskRepository _tasks;
    private readonly IComplianceRepository _compliance;
    private readonly IReferralDraftRepository _drafts;

    // =========================================================
    // FILE STORAGE
    // =========================================================
    // Kept intentionally so storage can be restored later.
    // =========================================================

    private readonly IReferralDocumentRepository _documentRepository;
    private readonly IFileStorage _fileStorage;

    private readonly IUnitOfWork _unitOfWork;

    public CreateReferralIntakeCommandHandler(
    IPatientRepository patients,
    IReferralRepository referrals,
    ILocationRepository locations,
    IServiceTypeRepository serviceTypes,
    IPatientTaskRepository tasks,
    IComplianceRepository compliance,
    IReferralDraftRepository drafts,
    IReferralDocumentRepository documentRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _referrals = referrals;
        _locations = locations;
        _serviceTypes = serviceTypes;
        _tasks = tasks;
        _compliance = compliance;
        _drafts = drafts;

        _documentRepository = documentRepository;
        _fileStorage = fileStorage;

        _unitOfWork = unitOfWork;
    }

    public async Task<CreateReferralIntakeResult> Handle(
        CreateReferralIntakeCommand request,
        CancellationToken cancellationToken)
    {
        ReferralDraft? draft = null;

        if (request.ReferralDraftId.HasValue)
        {
            draft =
                await _drafts.GetByIdAsync(
                    request.ReferralDraftId.Value,
                    cancellationToken);

            if (draft is null)
            {
                throw new KeyNotFoundException(
                    "Referral draft was not found.");
            }

            if (draft.Status != ReferralStatus.Draft)
            {
                throw new InvalidOperationException(
                    "This referral draft has already been completed.");
            }
        }

        // =========================================================
        // DUPLICATE CHECKS
        // =========================================================

        var existingPatient =
            await _patients.GetByMrnAsync(
                request.MRN,
                cancellationToken);

        if (existingPatient is not null)
        {
            throw new InvalidOperationException(
                $"A patient with MRN '{request.MRN}' already exists.");
        }

        var referralExists =
            await _referrals.ExistsByReferralNumberAsync(
                request.ReferralNumber,
                cancellationToken);

        if (referralExists)
        {
            throw new InvalidOperationException(
                $"Referral number '{request.ReferralNumber}' already exists.");
        }

        // =========================================================
        // DEFAULT LOCATION
        // =========================================================

        var location =
            await _locations.GetDefaultAsync(
                cancellationToken);

        if (location is null)
        {
            location =
                new Location(
                    "Main Office",
                    isDefault: true);

            await _locations.AddAsync(
                location,
                cancellationToken);
        }

        // =========================================================
        // PATIENT
        // =========================================================

        var patient =
            new Patient(
                request.MRN,
                request.FirstName,
                request.LastName);

        patient.ApplyReferralIntake(
            request.MiddleName,
            request.DateOfBirth,
            request.Gender,
            request.PrimaryDiagnosis,
            request.SecondaryDiagnosis,
            request.StreetAddress,
            request.City,
            request.State,
            request.ZipCode,
            request.PrimaryPhone,
            request.AlternatePhone,
            request.EmergencyContactName,
            request.EmergencyContactRelationship,
            request.EmergencyContactPhone,
            request.PrimaryInsurance,
            request.InsuranceMemberId,
            request.AuthorizationDate,
            request.ApprovedVisits,
            request.AuthorizationRequired,
            request.ReferringPhysician,
            request.PhysicianPhone,
            request.ReferralNotes,
            request.CoordinatorId,
            request.ClinicianId,
            request.SocDate);

        await _patients.AddAsync(
            patient,
            cancellationToken);

        // =========================================================
        // REFERRAL
        // =========================================================

        var referral =
            new Referral(
                request.ReferralNumber,
                request.ReferralDate,
                request.ReferralSource,
                request.Priority,
                location.LocationId,
                request.DisciplineId,
                request.VisitPriority,
                request.CaseStatus,
                request.PrimaryInsurance,
                request.InsuranceMemberId,
                request.AuthorizationDate,
                request.ApprovedVisits,
                request.AuthorizationRequired,
                request.ReferringPhysician,
                request.PhysicianPhone,
                request.SecondaryDiagnosis,
                request.ReferralNotes,
                request.InternalNotes);

        referral.ConvertToPatient(
            patient.PatientId);

        if (request.CoordinatorId.HasValue)
        {
            referral.Assign(
                request.CoordinatorId.Value);
        }

        await _referrals.AddAsync(
            referral,
            cancellationToken);

        // =========================================================
        // SERVICE ORDERS
        // =========================================================

        var activeServices =
            await _serviceTypes.GetActiveAsync(
                cancellationToken);

        foreach (
            var requestedService
            in request.OrderedServices
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase))
        {
            var service =
                activeServices.FirstOrDefault(
                    x => string.Equals(
                        x.Name,
                        requestedService,
                        StringComparison.OrdinalIgnoreCase));

            if (service is null)
                continue;

            var order =
                new PatientServiceOrder(
                    patient.PatientId,
                    service.ServiceTypeId,
                    null,
                    null,
                    false);

            await _serviceTypes.AddOrderAsync(
                order,
                cancellationToken);
        }

        // =========================================================
        // INITIAL WORKFLOW TASKS
        // =========================================================

        var coordinator =
            request.CoordinatorId;

        var baseDate =
            DateTime.UtcNow;

        //await AddTask(
        //    patient.PatientId,
        //    "Review Referral",
        //    "Review the submitted referral information and verify that the intake information is complete.",
        //    baseDate.AddHours(4),
        //    "/referrals",
        //    coordinator,
        //    cancellationToken);

        await AddTask(
            patient.PatientId,
            "Verify Insurance",
            "Verify insurance eligibility and authorization requirements.",
            baseDate.AddDays(1),
            $"/tracker/patient/{patient.PatientId}",
            coordinator,
            cancellationToken);

        await AddTask(
            patient.PatientId,
            "Verify Physician Orders",
            "Review and verify the referring physician orders.",
            baseDate.AddDays(1),
            $"/tracker/patient/{patient.PatientId}",
            coordinator,
            cancellationToken);

        // =========================================================
        // NOTE:
        // Physician Orders and SOC Scheduling are intentionally NOT
        // created as ComplianceRecord items here because they are
        // not columns in the Excel compliance checklist.
        //
        // SOC scheduling remains handled by the existing SOC command
        // and visit workflow when that functionality is used.
        // =========================================================

        // Insurance verification is represented once as a workflow confirmation;
        // the Insurance tab and Compliance tab operate on this same record.
        await AddCompliance(patient.PatientId, "INSURANCE_VERIFICATION", "Insurance eligibility and authorization were reviewed and confirmed.", cancellationToken);

        // =============================================================
        // COMPLIANCE REQUIREMENTS
        // =============================================================
        //
        // These requirements correspond to the Excel workflow.
        //
        // PHASE 1 - INTAKE & ADMISSION PREPARATION
        // =============================================================

        await AddCompliance(
            patient.PatientId,
            "PRE_AUTH_RECEIVED",
            "Pre-authorization has been received.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "PRO_RECEIVED",
            "PRO has been received.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "H_AND_P_RECEIVED",
            "H&P has been received.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "F2F_RECEIVED",
            "Face-to-Face documentation has been received.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "PCP_CONFIRMED",
            "Primary Care Physician has been confirmed.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "AVS_RECEIVED",
            "AVS has been received.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "SIGNED_CONSENTS",
            "Required consents have been signed.",
            cancellationToken);

        // =============================================================
        // PHASE 2 - CARE INITIATION & COMPLIANCE
        // =============================================================

        await AddCompliance(
            patient.PatientId,
            "SOC_COMPLIANT",
            "Start of Care was completed within the required timeframe.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "DME_MED_SUPPLY",
            "DME and medical supplies have been arranged or confirmed.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "NOA_FILED",
            "Notice of Admission has been filed.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "OASIS_SOC_COMPLETE",
            "OASIS and Start of Care documentation have been completed.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "QA_APPROVAL",
            "QA approval has been completed and the PO/POC is ready for faxing.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "ORDERS_SIGNED",
            "Orders have been signed by the physician.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "DOCS_UPLOADED",
            "Required documentation has been uploaded to the appropriate system.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "CASE_MIX_IDENTIFIED",
            "Case mix has been identified.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "CASE_MIX_ORDERS_SIGNED",
            "Case mix orders have been signed.",
            cancellationToken);

        await AddCompliance(
            patient.PatientId,
            "CASE_MIX_PLOTTED",
            "Case mix has been plotted.",
            cancellationToken);
        await AddCompliance(patient.PatientId, "PCP_PT_NOTIFIED", "PCP and PT notification has been confirmed when required.", cancellationToken);

        // =============================================================
        // PHASE 3 - ONGOING / TRANSITION / DISCHARGE
        // =============================================================

        await AddCompliance(patient.PatientId, "ROC", "Resumption of Care has been confirmed when applicable.", cancellationToken);
        await AddCompliance(patient.PatientId, "RECERTIFICATION", "Recertification has been completed when applicable.", cancellationToken);
        await AddCompliance(patient.PatientId, "DISCHARGE_SUMMARY_SIGNED", "Discharge summary has been signed when applicable.", cancellationToken);
        await AddCompliance(patient.PatientId, "NOMNC_SIGNED", "NOMNC has been signed when applicable.", cancellationToken);


        // =========================================================
        // PDF STORAGE / METADATA
        // =========================================================
        // Current MetadataOnly mode persists metadata only. A configured
        // IFileStorage provider (for example Azure Blob) stores the bytes.
        // =========================================================

        Guid? referralDocumentId = null;
        string? storageKey = null;

        if (!string.IsNullOrWhiteSpace(request.PdfFileName))
        {
            var contentType = string.IsNullOrWhiteSpace(request.PdfContentType)
                ? "application/pdf"
                : request.PdfContentType;

            var fileSize = request.PdfSize
                ?? (request.PdfStream?.CanSeek == true
                    ? request.PdfStream.Length
                    : 0);

            // MetadataOnly is the current deployment mode. When a real
            // provider such as Azure Blob is enabled, the same abstraction
            // stores the bytes and returns a storage key.
            if (_fileStorage.CanStore && request.PdfStream is not null)
            {
                var now = DateTime.UtcNow;
                var storedFile = await _fileStorage.SaveAsync(
                    request.PdfStream,
                    request.PdfFileName,
                    contentType,
                    $"Referrals/{now:yyyy}/{now:MM}/{referral.ReferralId}",
                    cancellationToken);

                storageKey = storedFile.StorageKey;
                fileSize = storedFile.Size;
            }

            var document = new ReferralDocument(
                referral.ReferralId,
                storageKey,
                request.PdfFileName,
                contentType,
                fileSize);

            await AddDocumentAsync(document, cancellationToken);
            referralDocumentId = document.ReferralDocumentId;
        }

        // =========================================================
        // DATABASE SAVE
        // =========================================================

        // Mark the source draft as converted BEFORE saving so the
        // status transition is persisted atomically with the referral.
        if (draft is not null)
        {
            draft.ConvertToPatient();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        // =========================================================
        // RESULT
        // =========================================================

        return new CreateReferralIntakeResult(
            patient.PatientId,
            referral.ReferralId,
            referral.ReferralNumber,

            // NULL because document storage is disabled.
            referralDocumentId,

            // NULL because no file is being stored.
            storageKey);
    }

    // =============================================================
    // ADD TASK
    // =============================================================

    private async Task AddTask(
        Guid patientId,
        string title,
        string description,
        DateTime dueDate,
        string pageRoute,
        Guid? assignedUserId,
        CancellationToken cancellationToken)
    {
        var task =
            new PatientTask(
                patientId,
                title,
                description,
                dueDate,
                pageRoute);

        if (assignedUserId.HasValue)
        {
            task.Assign(
                assignedUserId.Value);
        }

        await _tasks.AddAsync(
            task,
            cancellationToken);
    }

    // =============================================================
    // ADD COMPLIANCE
    // =============================================================

    private async Task AddCompliance(
        Guid patientId,
        string requirementCode,
        string notes,
        CancellationToken cancellationToken)
    {
        var compliance =
            new ComplianceRecord(
                patientId,
                requirementCode,
                notes);

        await _compliance.AddAsync(
            compliance,
            cancellationToken);
    }

    // =============================================================
    // ADD DOCUMENT
    // =============================================================

    private async Task AddDocumentAsync(
        ReferralDocument document,
        CancellationToken cancellationToken)
    {
        await _documentRepository.AddAsync(
            document,
            cancellationToken);
    }
}