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

    private readonly IFileStorage _fileStorage;

    private readonly IUnitOfWork _unitOfWork;

    public CreateReferralIntakeCommandHandler(
    IPatientRepository patients,
    IReferralRepository referrals,
    ILocationRepository locations,
    IServiceTypeRepository serviceTypes,
    IPatientTaskRepository tasks,
    IComplianceRepository compliance,
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
        CreateReferralIntakeValidator.Validate(request);

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

        foreach (var requestedService
         in request.OrderedServices
             .Where(x => !string.IsNullOrWhiteSpace(x))
             .Distinct(StringComparer.OrdinalIgnoreCase))
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

        // =========================================================
        // INITIAL COMPLIANCE
        // =========================================================

        await _compliance.AddAsync(
            new ComplianceRecord(
                patient.PatientId,
                "REFERRAL_DOCUMENT",
                "Referral document received and attached."),
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

        // =========================================================
        // STORE PDF
        // =========================================================

        var now = DateTime.UtcNow;

        var storageFolder =
            $"Referrals/{now:yyyy}/{now:MM}/{referral.ReferralId}";

        var storedFile = await _fileStorage.SaveAsync(
            request.PdfStream,
            request.PdfFileName,
            request.PdfContentType,
            storageFolder,
            cancellationToken);


        // =========================================================
        // DOCUMENT RECORD
        // =========================================================

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

        try
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            try
            {
                await _fileStorage.DeleteAsync(
                    storedFile.StorageKey,
                    cancellationToken);
            }
            catch
            {
                // Preserve the original database exception.
            }

            throw;
        }

        return new CreateReferralIntakeResult(
            patient.PatientId,
            referral.ReferralId,
            referral.ReferralNumber,
            document.ReferralDocumentId,
            storedFile.StorageKey);
    }

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

    

    private async Task AddDocumentAsync(
        ReferralDocument document,
        CancellationToken cancellationToken)
    {
        await _documentRepository.AddAsync(
            document,
            cancellationToken);
    }

    private readonly IReferralDocumentRepository
        _documentRepository;
}