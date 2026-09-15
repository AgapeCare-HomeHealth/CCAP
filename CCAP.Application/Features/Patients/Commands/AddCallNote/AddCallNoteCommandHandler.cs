using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;

namespace CCAP.Application.Features.Patients.Commands.AddCallNote;

public sealed class AddCallNoteCommandHandler : IRequestHandler<AddCallNoteCommand, Guid>
{
    private readonly IPatientRepository _patients;
    private readonly ICallNoteRepository _callNotes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientAuditLogRepository _auditLogs;

    public AddCallNoteCommandHandler(
        IPatientRepository patients,
        ICallNoteRepository callNotes,
        IUnitOfWork unitOfWork,
        IPatientAuditLogRepository auditLogs)
    {
        _patients = patients;
        _callNotes = callNotes;
        _unitOfWork = unitOfWork;
        _auditLogs = auditLogs;
    }

    public async Task<Guid> Handle(AddCallNoteCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdForWorkflowUpdateAsync(request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        var note = new CallNote(
            request.PatientId,
            request.RecordedByUserId,
            request.ContactType,
            request.Method,
            request.Subject,
            request.Notes,
            request.Outcome);

        await _callNotes.AddAsync(note, cancellationToken);
        patient.Activities.Add(new Activity(
            patient.PatientId, request.RecordedByUserId, "Communication",
            $"Communication recorded: {request.Method}",
            $"{request.ContactType} - {request.Subject}"));
        await _auditLogs.AddAsync(new PatientAuditLog(
            patient.PatientId, request.RecordedByUserId, "Communication",
            note.CallNoteId.ToString(), "CREATE", null,
            System.Text.Json.JsonSerializer.Serialize(new { note.CallNoteId, request.ContactType, request.Method, request.Subject, request.Notes, request.Outcome }),
            "Communication record created."), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return note.CallNoteId;
    }
}
