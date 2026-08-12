using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;

namespace CCAP.Application.Features.Patients.Commands.AddCallNote;

public sealed class AddCallNoteCommandHandler : IRequestHandler<AddCallNoteCommand, Guid>
{
    private readonly IPatientRepository _patients;
    private readonly ICallNoteRepository _callNotes;
    private readonly IUnitOfWork _unitOfWork;

    public AddCallNoteCommandHandler(
        IPatientRepository patients,
        ICallNoteRepository callNotes,
        IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _callNotes = callNotes;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddCallNoteCommand request, CancellationToken cancellationToken)
    {
        _ = await _patients.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException("Patient not found.");

        var note = new CallNote(
            request.PatientId,
            request.RecordedByUserId,
            request.Subject,
            request.Notes,
            request.Outcome);

        await _callNotes.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return note.CallNoteId;
    }
}
