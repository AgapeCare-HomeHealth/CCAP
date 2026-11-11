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
    private readonly IPatientCareLogRepository _careLogs;

    public GetPatientCareManagementQueryHandler(
        IPatientRepository patients,
        ICallNoteRepository callNotes,
        IServiceTypeRepository serviceTypes, IPatientCareLogRepository careLogs)
    {
        _patients = patients;
        _callNotes = callNotes;
        _serviceTypes = serviceTypes;
        _careLogs = careLogs;
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

        var careLogs = await _careLogs.GetByPatientIdAsync(patient.PatientId, cancellationToken);

        var mappedCareLogs = careLogs.Select(x => new PatientCareLogResponseDto {
            PatientCareLogId = x.PatientCareLogId, LogType = x.LogType, Item = x.Item, Quantity = x.Quantity, Unit = x.Unit,
            RecordedAt = x.RecordedAt, Notes = x.Notes, Status = x.Status, RecordedBy = x.RecordedByUser is null ? "System" : $"{x.RecordedByUser.FirstName} {x.RecordedByUser.LastName}".Trim()
        }).ToList();

        var serviceOrders =
            await _serviceTypes.GetOrdersByPatientIdAsync(
                patient.PatientId,
                cancellationToken);


        // ========================================================
        // MAP CALL NOTES
        // ========================================================

        var notes = callNotes
            .OrderByDescending(x => x.CallDate)
            .Select(x => new PatientNoteResponseDto
            {
                NoteId = x.CallNoteId,

                PatientId = x.PatientId,

                ContactType = x.ContactType,

                Method = x.Method,

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
                    x.IsPrimaryDiscipline,
                OrderDate = x.OrderDate
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
                    patient.PatientId,

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
        // OTHER PATIENT-RELATED NOTES
        // ========================================================
        // Keep these as notes, not timeline/audit events. Calendar notes are
        // read from the actual Visit record used by the scheduling calendar.
        var relatedNotes = new List<PatientRelatedNoteResponseDto>();

        foreach (var visit in patient.Visits)
        {
            var author = visit.Clinician is null
                ? (visit.ClinicianName ?? string.Empty)
                : $"{visit.Clinician.FirstName} {visit.Clinician.LastName}".Trim();

            if (!string.IsNullOrWhiteSpace(visit.CallNotes))
                relatedNotes.Add(new PatientRelatedNoteResponseDto { Source = "Calendar / Schedule", Title = $"{visit.VisitType} — Call Notes", Content = visit.CallNotes, Author = author, Date = visit.ScheduledDate });
            if (!string.IsNullOrWhiteSpace(visit.NotesFlag))
                relatedNotes.Add(new PatientRelatedNoteResponseDto { Source = "Calendar / Schedule", Title = $"{visit.VisitType} — Notes Flag", Content = visit.NotesFlag, Author = author, Date = visit.ScheduledDate });
            if (!string.IsNullOrWhiteSpace(visit.Notes))
                relatedNotes.Add(new PatientRelatedNoteResponseDto { Source = "Calendar / Schedule", Title = $"{visit.VisitType} — Schedule / Visit Notes", Content = visit.Notes, Author = author, Date = visit.CompletedDate ?? visit.ScheduledDate });
        }

        foreach (var assessment in patient.Assessments)
        {
            if (!string.IsNullOrWhiteSpace(assessment.Notes))
                relatedNotes.Add(new PatientRelatedNoteResponseDto { Source = "Assessment", Title = $"Assessment ({assessment.Status})", Content = assessment.Notes!, Author = assessment.CompletedBy is null ? string.Empty : $"{assessment.CompletedBy.FirstName} {assessment.CompletedBy.LastName}".Trim(), Date = assessment.CompletedAt });
        }

        void AddPatientFieldNote(string source, string title, string? content, DateTime? date = null)
        {
            if (!string.IsNullOrWhiteSpace(content))
                relatedNotes.Add(new PatientRelatedNoteResponseDto { Source = source, Title = title, Content = content.Trim(), Date = date });
        }

        // ReferralNotes is already shown as the canonical "Referral Notes" entry in OverviewTab.
        // Do not add it again here as "Patient Referral Notes".
        AddPatientFieldNote("Compliance", "DME / Medical Supply Notes", patient.DmeMedSupplyNotes);
        AddPatientFieldNote("Care", "SOC Feedback from Patient", patient.SocFeedbackFromPatient, patient.SocDate?.ToDateTime(TimeOnly.MinValue));
        AddPatientFieldNote("Discharge", "Discharge Feedback", patient.DischargeFeedback, patient.DischargeDate?.ToDateTime(TimeOnly.MinValue));
        AddPatientFieldNote("Transfer", "Transfer Reason", patient.TransferReason, patient.TransferDate?.ToDateTime(TimeOnly.MinValue));

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

            Tasks = tasks,
            CareLogs = mappedCareLogs,
            RelatedNotes = relatedNotes.OrderByDescending(x => x.Date ?? DateTime.MinValue).ToList()
        };
    }
}