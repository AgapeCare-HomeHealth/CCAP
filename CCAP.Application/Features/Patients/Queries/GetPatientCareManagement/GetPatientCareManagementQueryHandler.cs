using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Patients.DTOs;
using MediatR;

namespace CCAP.Application.Features.Patients.Queries.GetPatientCareManagement;

public sealed class GetPatientCareManagementQueryHandler
    : IRequestHandler<
        GetPatientCareManagementQuery,
        PatientCareProfileResponseDto?>
{
    private readonly IPatientRepository _patients;
    private readonly ICallNoteRepository _callNotes;
    private readonly IServiceTypeRepository _serviceTypes;

    public GetPatientCareManagementQueryHandler(
        IPatientRepository patients,
        ICallNoteRepository callNotes,
        IServiceTypeRepository serviceTypes)
    {
        _patients = patients;
        _callNotes = callNotes;
        _serviceTypes = serviceTypes;
    }

    public async Task<PatientCareProfileResponseDto?> Handle(
        GetPatientCareManagementQuery request,
        CancellationToken cancellationToken)
    {
        // ========================================================
        // PATIENT
        // ========================================================

        var patient = await _patients.GetByIdAsync(
            request.PatientId,
            cancellationToken);

        if (patient is null)
            return null;


        // ========================================================
        // CALL NOTES
        // ========================================================

        var callNotes = await _callNotes.GetByPatientIdAsync(
            patient.PatientId,
            cancellationToken);


        // ========================================================
        // SERVICE ORDERS
        // ========================================================

        var serviceOrders =
            await _serviceTypes.GetOrdersByPatientIdAsync(
                patient.PatientId,
                cancellationToken);


        // ========================================================
        // MAP CALL NOTES
        // ========================================================

        var notes = callNotes
            .OrderByDescending(x => x.CallDate)
            .Take(20)
            .Select(x => new PatientNoteResponseDto
            {
                NoteId = x.CallNoteId,

                PatientId = x.PatientId,

                Subject = x.Subject,

                Content = x.Notes,

                Priority = "Normal",

                CreatedBy = x.RecordedBy is null
                    ? "Unknown"
                    : $"{x.RecordedBy.FirstName} {x.RecordedBy.LastName}",

                CreatedAt = x.CallDate,

                Resolved = false,

                Outcome = x.Outcome
            })
            .ToList();


        // ========================================================
        // MAP SERVICE ORDERS
        // ========================================================

        var orders = serviceOrders
            .OrderByDescending(x => x.IsPrimaryDiscipline)
            .ThenBy(x => x.ServiceType.Name)
            .Select(x => new PatientServiceOrderResponseDto
            {
                PatientServiceOrderId =
                    x.PatientServiceOrderId,

                PatientId =
                    x.PatientId,

                ServiceTypeId =
                    x.ServiceTypeId,

                ServiceCode =
                    x.ServiceType.Code,

                ServiceName =
                    x.ServiceType.Name,

                Status =
                    x.Status,

                Frequency =
                    x.Frequency ?? string.Empty,

                Duration =
                    x.Duration ?? string.Empty,

                IsPrimaryDiscipline =
                    x.IsPrimaryDiscipline
            })
            .ToList();


        // ========================================================
        // UPCOMING VISITS
        // ========================================================

        var upcomingVisits = patient.Visits
            .Where(x =>
                x.ScheduledDate >= DateTime.UtcNow &&
                !string.Equals(
                    x.Status,
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ScheduledDate)
            .Take(10)
            .Select(x => new VisitResponseDto
            {
                VisitId =
                    x.VisitId,

                PatientId =
                    x.PatientId,

                ScheduledDate =
                    x.ScheduledDate,

                CompletedDate =
                    x.CompletedDate,

                Status =
                    x.Status,

                AssignedTo =
                    x.Clinician is null
                        ? "Unassigned"
                        : $"{x.Clinician.FirstName} {x.Clinician.LastName}",

                Notes =
                    x.Notes
            })
            .ToList();


        // ========================================================
        // PATIENT TASKS
        // ========================================================

        var tasks = patient.Tasks
            .Where(x =>
                !string.Equals(
                    x.Status.ToString(),
                    "Completed",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    x.Status.ToString(),
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.DueDate)
            .Take(20)
            .Select(x => new PatientTaskResponseDto
            {
                TaskId =
                    x.TaskId,

                PatientId =
                    x.PatientId,

                Title =
                    x.Title,

                Description =
                    x.Description,

                DueDate =
                    x.DueDate,

                Status =
                    x.Status.ToString(),

                AssignedTo =
                    x.AssignedUser is null
                        ? "Unassigned"
                        : $"{x.AssignedUser.FirstName} {x.AssignedUser.LastName}",

                PageRoute =
                    x.PageRoute,

                IsOverdue =
                    x.DueDate < DateTime.UtcNow
            })
            .ToList();


        // ========================================================
        // RETURN RESPONSE
        // ========================================================

        return new PatientCareProfileResponseDto
        {
            PatientId =
                patient.PatientId,

            // ----------------------------------------------------
            // There is currently no Fax domain entity.
            // Therefore we deliberately return an empty object.
            // ----------------------------------------------------

            Fax = new FaxInformationResponseDto
            {
                PatientId =
                    patient.PatientId,

                FaxId =
                    Guid.Empty,

                FaxNumber =
                    string.Empty,

                ReferringProvider =
                    patient.ReferringPhysician ?? string.Empty,

                Organization =
                    string.Empty,

                DocumentType =
                    string.Empty,

                ReceivedAt =
                    null,

                Verified =
                    false,

                Notes =
                    null
            },

            // No Notification entity currently exists.
            Notifications = [],

            Notes = notes,

            ServiceOrders = orders,

            UpcomingVisits = upcomingVisits,

            Tasks = tasks
        };
    }
}