using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Storage;
using CCAP.Domain.Entities;
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

        // =====================================================
        // FILE STORAGE
        // =====================================================
        // Keep these dependencies for easy re-enabling later.
        // =====================================================

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

        _documentRepository = documentRepository;
        _fileStorage = fileStorage;

        _unitOfWork = unitOfWork;
    }


    public async Task<CreateReferralIntakeResult> Handle(
        CreateReferralIntakeCommand request,
        CancellationToken cancellationToken)
    {
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


        await AddTask(
            patient.PatientId,
            "Review Referral",
            "Review the submitted referral information and verify that the intake information is complete.",
            baseDate.AddHours(4),
            "/referrals",
            coordinator,
            cancellationToken);


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


        await AddTask(
            patient.PatientId,
            "Schedule SOC Visit",
            "Schedule the patient's Start of Care visit.",
            request.SocDate
                .Value
                .ToDateTime(TimeOnly.MinValue),
            $"/tracker/patient/{patient.PatientId}",
            coordinator,
            cancellationToken);


        // =============================================================
        // COMPLIANCE REQUIREMENTS
        // =============================================================

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "REFERRAL_DOCUMENT",
                "Referral document has not been uploaded or stored."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "INSURANCE_VERIFICATION",
                "Insurance verification is required."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "PHYSICIAN_ORDERS",
                "Physician orders must be reviewed."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "SOC_SCHEDULING",
                "Start of Care visit must be scheduled."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "SOC_COMPLIANT",
                "Start of Care must be completed within the required timeframe."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "DME_MED_SUPPLY",
                "DME and required medical supplies must be arranged or confirmed."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "NOA_FILED",
                "Notice of Admission must be filed."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "OASIS_SOC_COMPLETE",
                "OASIS and Start of Care documentation must be completed."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "QA_APPROVAL",
                "QA approval is required before the PO/POC is ready for faxing."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "ORDERS_SIGNED",
                "Orders must be signed by the physician."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "DOCS_UPLOADED",
                "Required documentation must be uploaded to the appropriate system."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "SOC_FEEDBACK_QA",
                "SOC feedback from QA must be completed."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "CASE_MIX_IDENTIFIED",
                "Case mix must be identified."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "CASE_MIX_ORDERS_SIGNED",
                "Case mix orders must be signed."),
            cancellationToken);

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "CASE_MIX_PLOTTED",
                "Case mix must be plotted."),
            cancellationToken);


        // =========================================================
        // PDF STORAGE
        // =========================================================
        //
        // TEMPORARILY DISABLED
        //
        // The PDF may still be received by the API, but it is
        // intentionally NOT written to disk, Plesk, cloud storage,
        // or the database.
        //
        // To restore storage later, uncomment the block below.
        //
        // =========================================================

        Guid? referralDocumentId = null;

        string? storageKey = null;


        /*
        // =========================================================
        // RESTORE PDF STORAGE HERE
        // =========================================================

        if (request.PdfStream is not null)
        {
            var now =
                DateTime.UtcNow;

            var storageFolder =
                $"Referrals/{now:yyyy}/{now:MM}/{referral.ReferralId}";


            var storedFile =
                await _fileStorage.SaveAsync(
                    request.PdfStream,
                    request.PdfFileName!,
                    request.PdfContentType!,
                    storageFolder,
                    cancellationToken);


            storageKey =
                storedFile.StorageKey;


            // =====================================================
            // CREATE DOCUMENT RECORD
            // =====================================================

            var document =
                new ReferralDocument(
                    referral.ReferralId,
                    storedFile.StorageKey,
                    storedFile.OriginalFileName,
                    storedFile.ContentType,
                    storedFile.Size);


            await AddDocumentAsync(
                document,
                cancellationToken);


            referralDocumentId =
                document.ReferralDocumentId;
        }
        */


        // =========================================================
        // DATABASE SAVE
        // =========================================================

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
    // ADD DOCUMENT
    // =============================================================
    //
    // Currently unused because PDF storage is disabled.
    //
    // Keep it here so restoring document storage later is easy.
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