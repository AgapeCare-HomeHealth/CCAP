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
    private readonly IUserRepository _users;

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
    IUserRepository users,
    IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _referrals = referrals;
        _locations = locations;
        _serviceTypes = serviceTypes;
        _tasks = tasks;
        _compliance = compliance;
        _drafts = drafts;
        _users = users;

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

        if (!request.CoordinatorId.HasValue)
            throw new InvalidOperationException("A Primary CC must be assigned before submitting the referral.");

        var coordinatorIds = new[] { request.CoordinatorId, request.SecondaryCoordinatorId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        if (request.CoordinatorId.HasValue && request.CoordinatorId == request.SecondaryCoordinatorId)
            throw new InvalidOperationException("Primary and Secondary CC must be different users.");

        if (coordinatorIds.Count > 0)
        {
            var selectedCoordinators = await _users.GetByIdsAsync(coordinatorIds, cancellationToken);
            if (selectedCoordinators.Count != coordinatorIds.Count ||
                selectedCoordinators.Any(u => !u.IsActive ||
                    !string.Equals(u.Role?.RoleName, "Care Coordinator", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Primary and Secondary CC must be active Care Coordinator users.");
        }

        // =========================================================
        // DEFAULT IDENTIFIERS
        // =========================================================
        // MRN and referral number are system identifiers. They are not
        // intake requirements because the EMR may contain them already
        // or the referral document may not provide them. Generate stable
        // values when they are absent.
        // =========================================================

        var mrn = string.IsNullOrWhiteSpace(request.MRN)
            ? $"MRN-{Guid.NewGuid():N}"
            : request.MRN.Trim();

        var referralNumber = string.IsNullOrWhiteSpace(request.ReferralNumber)
            ? $"REF-{Guid.NewGuid():N}"
            : request.ReferralNumber.Trim();

        var referralDate = request.ReferralDate == default
            ? DateTime.UtcNow
            : request.ReferralDate;

        // =========================================================
        // DUPLICATE CHECKS
        // =========================================================

        var existingPatient =
            await _patients.GetByMrnAsync(
                mrn,
                cancellationToken);

        if (existingPatient is not null)
        {
            throw new InvalidOperationException(
                $"A patient with MRN '{mrn}' already exists.");
        }

        var referralExists =
            await _referrals.ExistsByReferralNumberAsync(
                referralNumber,
                cancellationToken);

        if (referralExists)
        {
            throw new InvalidOperationException(
                $"Referral number '{referralNumber}' already exists.");
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
                mrn,
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
            request.SecondaryCoordinatorId,
            request.ClinicianId,
            request.SocDate);

        patient.SetEmergencyContactsJson(request.EmergencyContactsJson);

        await _patients.AddAsync(
            patient,
            cancellationToken);

        // =========================================================
        // REFERRAL
        // =========================================================
        // Target SOC is not user-entered. It is always calculated as
        // 48 hours from the initial referral date.
        var targetSocDate = DateOnly.FromDateTime(referralDate.AddHours(48));

        var referral =
            new Referral(
                referralNumber,
                referralDate,
                request.ReferralSource,
                request.ReferralType,
                request.ReferralChannel,
                request.Priority,
                location.LocationId,
                request.DisciplineId,
                request.VisitPriority,
                "Pending",
                request.PrimaryInsurance,
                request.InsuranceMemberId,
                request.AuthorizationDate,
                request.ApprovedVisits,
                request.AuthorizationRequired,
                request.ReferringPhysician,
                request.PhysicianPhone,
                request.PhysicianFax,
                request.PhysicianNpi,
                request.PcosRegistered,
                request.SecondaryDiagnosis,
                request.ReferralNotes,
                request.InternalNotes);

        referral.ConvertToPatient(
            patient.PatientId);

        referral.SetInitialTrackerDetails(
            request.PostedOn,
            request.TrackerStatus,
            request.EstimatedDischargeDate,
            request.TrackerCoordinatorName,
            request.FirstMarketingCall,
            request.SecondCoordinatorCall,
            request.ThirdCoordinatorCall,
            request.Resolution);

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
            // Referral intake labels are not always identical to the normalized
            // service type names (e.g. "Medical Social Work (MSW)" vs
            // "Medical Social Worker (MSW)"). Resolve by normalized label/code.
            var normalizedRequested = requestedService
                .Replace("Medical Social Work", "Medical Social Worker", StringComparison.OrdinalIgnoreCase)
                .Trim();

            var service = activeServices.FirstOrDefault(x =>
                string.Equals(x.Name, requestedService, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Name, normalizedRequested, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Code, requestedService, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(x.Code, "MSW", StringComparison.OrdinalIgnoreCase) &&
                 requestedService.Contains("MSW", StringComparison.OrdinalIgnoreCase)));

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

        //await AddTask(
        //    patient.PatientId,
        //    "Review Referral",
        //    "Review the submitted referral information and verify that the intake information is complete.",
        //    baseDate.AddHours(4),
        //    "/referrals",
        //    coordinator,
        //    cancellationToken);


        // Physician orders are tracked only as a ComplianceRecord below.
        // Do not create a separate PatientTask that cannot be completed from
        // the Compliance tab.

        // =========================================================
        // NOTE:
        // Physician orders are also exposed as a ComplianceRecord so the
        // overdue workflow item has a matching, completable item in the
        // patient's Compliance tab. SOC scheduling remains handled by the
        // existing SOC command and visit workflow.
        // =========================================================

        // Insurance remains a separate workflow stage and is verified from
        // the Insurance tab. The checklist below starts after Insurance.
        await AddCompliance(patient.PatientId, "INSURANCE_VERIFICATION", "Complete insurance verification before Admission.", cancellationToken);

        // ADMISSION
        await AddCompliance(patient.PatientId, "PRE_AUTH_RECEIVED", "Confirm pre-authorization has been received.", cancellationToken);
        await AddCompliance(patient.PatientId, "PRO_RECEIVED", "Confirm PRO has been received.", cancellationToken);
        await AddCompliance(patient.PatientId, "H_AND_P_RECEIVED", "Confirm H&P has been received.", cancellationToken);
        await AddCompliance(patient.PatientId, "F2F_RECEIVED", "Confirm Face-to-Face documentation has been received.", cancellationToken);
        await AddCompliance(patient.PatientId, "PCP_CONFIRMED", "Confirm the primary care physician.", cancellationToken);

        // SOC
        await AddCompliance(patient.PatientId, "DME_MED_SUPPLY", "Confirm DME and medical supplies.", cancellationToken);
        await AddCompliance(patient.PatientId, "SIGNED_CONSENTS", "Confirm required consents are signed.", cancellationToken);
        await AddCompliance(patient.PatientId, "NOA_FILED", "Confirm the Notice of Admission is filed.", cancellationToken);
        await AddCompliance(patient.PatientId, "OASIS_SOC_COMPLETE", "Confirm OASIS / SOC compliance.", cancellationToken);
        await AddCompliance(patient.PatientId, "ORDERS_SIGNED", "Confirm orders are signed by the physician.", cancellationToken);
        await AddCompliance(patient.PatientId, "DOCS_UPLOADED", "Confirm required documents are uploaded.", cancellationToken);
        await AddCompliance(patient.PatientId, "AVS_RECEIVED", "Confirm AVS has been received.", cancellationToken);

        // ON-GOING CARE
        await AddCompliance(patient.PatientId, "CASE_MIX_IDENTIFIED", "Confirm case mix has been identified.", cancellationToken);
        await AddCompliance(patient.PatientId, "CASE_MIX_PLOTTED", "Confirm case mix has been plotted.", cancellationToken);
        await AddCompliance(patient.PatientId, "QA_APPROVAL", "Confirm QA approval and PO/POC readiness for faxing.", cancellationToken);

        // RECERT OR DISCHARGE
        await AddCompliance(patient.PatientId, "PCP_PT_NOTIFIED", "Confirm PCP and PT have been notified.", cancellationToken);

        // DISCHARGE
        await AddCompliance(patient.PatientId, "DISCHARGE_SUMMARY_SIGNED", "Confirm the discharge summary is signed.", cancellationToken);
        await AddCompliance(patient.PatientId, "NOMNC_SIGNED", "Confirm NOMNC is signed.", cancellationToken);

        // Every compliance requirement is represented by an assigned user task.
        // Tasks are linked by a stable title key so completion/reverification
        // stays synchronized without adding a second source of truth.
        // Use the same idempotent task synchronizer used by the patient workflow.
        // This creates one task per actionable compliance requirement plus the
        // separate Insurance Verification task, assigns them to the Primary CC,
        // and prevents duplicate task creation if this workflow is retried.
        await CCAP.Application.Features.Patients.PatientComplianceTaskSynchronizer
            .EnsureAllAsync(patient, _tasks, _compliance, cancellationToken);


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
